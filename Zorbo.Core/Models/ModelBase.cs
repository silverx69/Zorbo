using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Zorbo.Core.Models
{
    [JsonObject]
    public abstract class ModelBase : INotifyPropertyChanged
    {
        protected void OnPropertyChanged<T>(Expression<Func<T>> fieldSelector, T newValue, [CallerMemberName] string propertyName = null)
        {
            if (!(fieldSelector.Body is MemberExpression body))
                throw new ArgumentException("fieldSelector", "Field selector must be a member access expression.");

            var member = body.Member as FieldInfo;
            if (member == null) throw new InvalidOperationException("Field selector must return a field.");

            T oldValue = (T)member.GetValue(this);

            if (!Equals(oldValue, newValue)) {
                member.SetValue(this, newValue);
                RaisePropertyChanged(propertyName);
            }
        }

        protected void OnPropertyChanged<T>(Expression<Func<T>> propSelector, Expression<Func<T>> fieldSelector, T newValue)
        {
            if (!(fieldSelector.Body is MemberExpression body))
                throw new ArgumentException("fieldSelector", "Field selector must be a member access expression.");

            var member = body.Member as FieldInfo;
            if (member == null) throw new InvalidOperationException("Field selector must return a field.");

            T oldValue = (T)member.GetValue(this);

            if (!Equals(oldValue, newValue)) {
                body = propSelector.Body as MemberExpression;
                if (body == null) throw new ArgumentException("propSelector", "Property selector must be a member access expression.");

                var prop = body.Member as PropertyInfo;
                if (prop == null) throw new InvalidOperationException("Property selector must return a property.");

                member.SetValue(this, newValue);
                RaisePropertyChanged(prop.Name);
            }
        }

        protected void OnPropertyChanged<T>(Expression<Func<T>> propSelector)
        {
            if (!(propSelector.Body is MemberExpression body))
                throw new ArgumentException("propSelector", "Property selector must be a member access expression.");

            var prop = body.Member as PropertyInfo;
            if (prop == null) throw new InvalidOperationException("Property selector must return a property.");

            RaisePropertyChanged(prop.Name);
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) 
        {
            RaisePropertyChanged(propertyName);
        }

        protected virtual void RaisePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
