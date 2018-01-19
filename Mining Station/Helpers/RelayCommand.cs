using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Mining_Station
{
    public class RelayCommand : ICommand
    {
        private Action<object> executeDelegate;
        readonly Predicate<object> canExecuteDelegate;

        public RelayCommand(Action<object> execute, Predicate<object> canExecute)
        {
            if (execute == null)
                throw new NullReferenceException("execute");
            executeDelegate = execute;
            canExecuteDelegate = canExecute;
        }

        public RelayCommand(Action<object> execute) : this(execute, null) { }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object parameter)
        {
            return canExecuteDelegate == null ? true : canExecuteDelegate(parameter);
        }

        public void Execute(object parameter)
        {
            executeDelegate.Invoke(parameter);
        }
    }
}
