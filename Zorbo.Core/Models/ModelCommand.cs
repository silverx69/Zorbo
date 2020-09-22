using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;

namespace Zorbo.Core.Models
{
    public sealed class ModelCommand : ICommand
    {
        readonly Action<object> m_execute;
        readonly Predicate<object> m_canExecute;

        public ModelCommand(Action<object> execute, Predicate<object> canExecute = null)
        {
            m_execute = execute ?? throw new ArgumentNullException("execute");
            m_canExecute = canExecute;
        }

        public bool CanExecute(object parameter)
        {
            return m_canExecute?.Invoke(parameter) ?? true;
        }

        public void Execute(object parameter)
        {
            m_execute.Invoke(parameter);
        }

        public void Update()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler CanExecuteChanged;
    }
}
