using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using MyDemo.ViewModel;

namespace MyDemo.ViewModel.Command
{
    internal class StartServerCommand : ICommand
    {
        private MainWindowViewModel parent = null;


        public StartServerCommand(MainWindowViewModel parent)
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
            parent.StartChatServer();
        }
    }
}
