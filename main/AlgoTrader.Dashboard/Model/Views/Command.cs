using System;
using System.Windows.Input;

namespace AlgoTrader.Dashboard.Model.Views
{
    public class Command : ICommand
    {
        private readonly Action _execute;

        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        public Command(Action execute)
        {
            _execute = execute ?? throw new ArgumentNullException("execute");
        }
        
        public bool CanExecute(object parameters) => true;

        public void Execute(object parameters) => _execute();
    }

    public class Command<T> : ICommand where T : class
    {
        private readonly Action<T> _execute;
        private readonly Predicate<T> _canExecute;

        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        public Command(Action<T> execute)
        {
            _execute = execute ?? throw new ArgumentNullException("execute");
        }

        public Command(Action<T> execute, Predicate<T> canExecute) : this(execute)
        {
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameters) => _canExecute == null ? true : _canExecute(parameters as T);
        
        public void Execute(object parameters) => _execute(parameters as T);
    }
}
