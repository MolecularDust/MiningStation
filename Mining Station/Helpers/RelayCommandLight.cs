using System;
using System.Windows.Input;

namespace Mining_Station
{
    public class RelayCommandLight : ICommand
    {
        private Action<object> executeDelegate;
        private Predicate<object> canExecuteDelegate;

        public RelayCommandLight(Action<object> execute, Predicate<object> canExecute)
        {
            if (execute == null)
                throw new NullReferenceException("execute");
            executeDelegate = execute;
            canExecuteDelegate = canExecute;
        }

        public RelayCommandLight(Action<object> execute) : this(execute, null) { }

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return canExecuteDelegate == null ? true : canExecuteDelegate(parameter);
        }

        public void Execute(object parameter)
        {
            executeDelegate.Invoke(parameter);
        }

        public void SetCanExecute(bool canExecute)
        {
            canExecuteDelegate = b => canExecute;
            RaiseCanExecuteChanged();
        }

        public void RaiseCanExecuteChanged()
        {
            if (CanExecuteChanged != null)
            {
                CanExecuteChanged(this, EventArgs.Empty);
            }
        }
    }
}
