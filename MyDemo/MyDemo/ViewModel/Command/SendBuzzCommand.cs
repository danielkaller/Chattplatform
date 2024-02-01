﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MyDemo.ViewModel.Command
{
    internal class SendBuzzCommand : ICommand
    {
        private ChatWindowViewModel parent = null;

        public SendBuzzCommand(ChatWindowViewModel parent)
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
            parent.SendBuzz();
        }
    }
}
