using System;
using System.Windows.Input;

namespace StealthGame.ViewModel
{
    /// <summary>
    /// Command type.
    /// </summary>
    public class DelegateCommand : ICommand
    {
        private readonly Action<object> _execute; // executes the action
        private readonly Func<object, bool> _canExecute; // checks the condition of the action

        /// <summary>
        /// Creates command.
        /// </summary>
        /// <param name="execute">Executable action.</param>
        public DelegateCommand(Action<object> execute) : this(null, execute) { }

        /// <summary>
        /// Creates command.
        /// </summary>
        /// <param name="canExecute">Condition of the execution.</param>
        /// <param name="execute">Executable action.</param>
        public DelegateCommand(Func<object, bool> canExecute, Action<object> execute)
        {
            _execute = execute ?? throw new ArgumentNullException("execute");
            _canExecute = canExecute;
        }

        /// <summary>
        /// Executable changed event.
        /// </summary>
        public event EventHandler CanExecuteChanged;

        /// <summary>
        /// Checks execution.
        /// </summary>
        /// <param name="parameter">Action parameter.</param>
        /// <returns>True if the action is executable, otherwise false.</returns>
        public bool CanExecute(object parameter)
        {
            return _canExecute == null || _canExecute(parameter);
        }

        /// <summary>
        /// Executes the action.
        /// </summary>
        /// <param name="parameter">Action parameter.</param>
        public void Execute(object parameter)
        {
            if (!CanExecute(parameter))
            {
                throw new InvalidOperationException("Command execution is disabled.");
            }
            _execute(parameter);
        }

        /// <summary>
        /// Fires executable changed event.
        /// </summary>
        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
