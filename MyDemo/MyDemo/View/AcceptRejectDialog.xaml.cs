using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace MyDemo.View
{
    public partial class AcceptRejectDialog : Window
    {
        public AcceptRejectDialog()
        {
            InitializeComponent();
        }

        private void Accept_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true; // Set DialogResult to true when the "Accept" button is clicked.
            Close(); // Close the dialog.
        }

        private void Reject_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false; // Set DialogResult to false when the "Reject" button is clicked.
            Close(); // Close the dialog.
        }
    }
}