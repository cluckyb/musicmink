using System;
using System.Windows.Input;

namespace MusicMink.Common
{
    public class RelayCommand : ICommand
    {
        private readonly Predicate<object> _canExecute = null;
        private readonly Action<object> _execute = null;

        public RelayCommand(Predicate<object> canExecute, Action<object> execute)
        {
            _canExecute = canExecute;
            _execute = execute;
        }

        public bool CanExecute(object parameter)
        {
            if (_canExecute != null)
            {
                return (_canExecute(parameter));
            }

            return false;
        }

        public event EventHandler CanExecuteChanged;

        public void RaiseExecuteChanged()
        {
            if (CanExecuteChanged != null)
            {
                CanExecuteChanged(this, EventArgs.Empty);
            }
        }

        public void Execute(object parameter)
        {
            if (_execute != null)
            {
                _execute(parameter);
            }
        }
    }
}
