using MyDemo.ViewModel;
using System;
using System.Windows.Input;

namespace MyDemo.ViewModel.Command
{
    internal class CloseWindowCommand : ICommand
    {
        private ChatWindowViewModel parent = null;

        public CloseWindowCommand(ChatWindowViewModel parent)
        {
            this.parent = parent;

        }

        public event EventHandler? CanExecuteChanged;
        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            parent.CloseWindow();
        }
    }
}

