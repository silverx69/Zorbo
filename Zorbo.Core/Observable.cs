using System.ComponentModel;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Zorbo
{
    public abstract class Observable : IObservable
    {
        /// <summary>
        /// Uses lambda expressions to select a field and modify the field using Reflection with a new value. 
        /// If the value has changed, the PropertyChanged event will be raised using the CallerMemberName (ie, from inside a property 'set').
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="fieldSelector">An expression that selects the field being modified.</param>
        /// <param name="newValue">The new value of the field.</param>
        /// <param name="propertyName">The name of the property being changed. Compiler attribute CallerMemberName will be auto-filled if not supplied.</param>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        protected virtual void OnPropertyChanged<T>(Expression<Func<T>> fieldSelector, T newValue, [CallerMemberName] string propertyName = null) {
            if (fieldSelector.Body is not MemberExpression body)
                throw new ArgumentException("Field selector must be a member access expression.", nameof(fieldSelector));

            var member = body.Member as FieldInfo ??
                throw new InvalidOperationException("Field selector must return a field.");

            T oldValue = (T)member.GetValue(this);

            if (!Equals(oldValue, newValue)) {
                member.SetValue(this, newValue);
                OnPropertyChanged(propertyName);
            }
        }

        /// <summary>
        /// Uses lambda expressions to select a field and modify the field using Reflection with a new value. 
        /// If the value has changed, the PropertyChanged event will be raised using the name of the property.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="propSelector">An expression that selects the property that changed.</param>
        /// <param name="fieldSelector">An expression that selects the field being modified.</param>
        /// <param name="newValue">The new value of the field.</param>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        protected virtual void OnPropertyChanged<T>(Expression<Func<T>> propSelector, Expression<Func<T>> fieldSelector, T newValue) {
            if (fieldSelector.Body is not MemberExpression body)
                throw new ArgumentException("Field selector must be a member access expression.", nameof(fieldSelector));

            var member = body.Member as FieldInfo ??
                throw new InvalidOperationException("Field selector must return a field.");

            T oldValue = (T)member.GetValue(this);

            if (!Equals(oldValue, newValue)) {

                body = propSelector.Body as MemberExpression ??
                    throw new ArgumentException("Property selector must be a member access expression.", nameof(propSelector));

                var prop = body.Member as PropertyInfo ??
                    throw new InvalidOperationException("Property selector must return a property.");

                member.SetValue(this, newValue);
                OnPropertyChanged(prop.Name);
            }
        }
        /// <summary>
        /// Uses a lambda expression to raise the PropertyChanged event using the name of the property selected.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="propSelector">The property that changed.</param>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        protected virtual void OnPropertyChanged<T>(Expression<Func<T>> propSelector) {
            if (propSelector.Body is not MemberExpression body)
                throw new ArgumentException("Property selector must be a member access expression.", nameof(propSelector));

            var prop = body.Member as PropertyInfo ??
                throw new InvalidOperationException("Property selector must return a property.");

            OnPropertyChanged(prop.Name);
        }

        /// <summary>
        /// Raises the PropertyChanged event using the CallerMemberName.
        /// </summary>
        /// <param name="propertyName">The name of the property that changed. Compiler attribute CallerMemberName will be auto-filled if not supplied.</param>
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) {
            if (!string.IsNullOrEmpty(propertyName))
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        //INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
    }
}
