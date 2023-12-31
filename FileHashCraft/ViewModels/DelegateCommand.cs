using System.Windows.Input;

namespace FileHashCraft.ViewModels
{
    /// <summary>
    /// 型なしの ICommand実装
    /// </summary>
    public class DelegateCommand : ICommand
    {
        private readonly Action _execute;
        private readonly Func<bool>? _canExecute;

        public event EventHandler? CanExecuteChanged;

        public DelegateCommand()
        {
            throw new NotImplementedException();
        }

        public DelegateCommand(Action execute, Func<bool>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public bool CanExecute(object? parameter)
        {
            return _canExecute == null || _canExecute();
        }

        public void Execute(object? parameter)
        {
            _execute();
        }

        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    /// <summary>
    /// 型ありの ICommand実装
    /// </summary>
    public class DelegateCommand<T> : ICommand
    {
        private readonly Action<T> _execute;
        private readonly Func<T, bool>? _canExecute;

        public event EventHandler? CanExecuteChanged;

        public DelegateCommand()
        {
            throw new NotImplementedException();
        }

        public DelegateCommand(Action<T> execute, Func<T, bool>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public bool CanExecute(object? parameter)
        {
            return _canExecute == null || (parameter != null && _canExecute((T)parameter));
        }

        public void Execute(object? parameter)
        {
            if (parameter != null)
            {
                _execute((T)parameter);
            }
        }

        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }

        /* 
        //使い方
        public class YourViewModel : ObservableObject
        {
            public YourViewModel()
            {
                YourCommand = new DelegateCommand<string>(ExecuteYourCommand, CanExecuteYourCommand);
            }

            public ICommand YourCommand { get; }

            private void ExecuteYourCommand(string parameter)
            {
                // Your command logic here
            }

            private bool CanExecuteYourCommand(string parameter)
            {
                // Your can execute logic here
                return true;
            }
        }

        */
    }
}