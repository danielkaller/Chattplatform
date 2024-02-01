using MyDemo.Model;
using System;
using System.Windows;
using System.ComponentModel;
using MyDemo.ViewModel.Command;
using System.Net.Sockets;
using System.Threading;
using System.Windows.Input;
using MyDemo.View;

namespace MyDemo.ViewModel
{
    internal class MainWindowViewModel : INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler PropertyChanged;
        private NetworkManager NetworkManager { get; set; }
        private ICommand _startClient;
        private ICommand _startServer;
 

        private string textName; // To store text from TextBox_TextChanged
        private string textPort; // To store text from TextBox_TextChanged_1
        private string textIp; // To store text from TextBox_TextChanged_2
        public string textInfo;


        public MainWindowViewModel(NetworkManager networkManager)
        {
            NetworkManager = networkManager;
            NetworkManager.ServerOccupied += NetworkManager_ServerOccupied;
            NetworkManager.RequestDeclined += NetworkManager_RequestDeclined;
            NetworkManager.RequestWait += NetworkManager_RequestWait;
            NetworkManager.ServerDown += NetworkManager_ServerDown;
            NetworkManager.PromptAcceptClient += NetworkManager_PromptAcceptClient;
        }

        public string TextName
        {
            get { return textName; }
            set
            {
                textName = value;
            }
        }

        public string TextPort
        {
            get { return textPort; }
            set
            {
                textPort = value;
            }
        }

        public string TextIp
        {
            get { return textIp; }
            set
            {
                textIp = value;
            }
        }

        public string TextInfo
        {
            get { return textInfo; }
            set
            {
                textInfo = value;
                OnPropertyChanged(nameof(TextInfo));
            }
        }

        public ICommand startClient
        {
            get
            {
                if (_startClient == null)
                    _startClient = new StartClientCommand(this);
                return _startClient;
            }
            set
            {
                _startClient = value;
            }
        }

        public ICommand startServer
        {
            get
            {
                if (_startServer == null)
                    _startServer = new StartServerCommand(this);
                return _startServer;
            }
            set
            {
                _startServer = value;
            }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void NetworkManager_ServerOccupied(object sender, string throwAway)
        {
            //Subscribe event for when a Server is occupied
            string info = $"Server with port number: {TextPort} and ip: {TextIp} is occupied";
            SendInfo(info);

        }
        private void NetworkManager_RequestDeclined(object sender, string name)
        {
            //Subscribe event for when a Request is Declined
            string info = $"{name} declined your request";
            SendInfo(info);

        }

        public void NetworkManager_RequestWait(object sender, string throwAway)
        {
            //Subscribe event for when waiting for a Request Response
            string info = $"Waiting for user response";
            SendInfo(info);

        }

        private void NetworkManager_ServerDown(object sender, string throwAway)
        {
            //Subscribe event for when a Server is not online
            string info = $"Server with port number: {TextPort} and ip: {TextIp} is not online";
            SendInfo(info);

        }

        private void SendInfo(string info)
        {
            //Our Feedback sender
            Application.Current.Dispatcher.Invoke(() =>
            {
                TextInfo = info;
            });
        }

        public void StartChatServer()
        {
            //Start the server on a separate thread
            Thread serverTask = new Thread(() =>
            {
                NetworkManager.StartConnection(textName, int.Parse(textPort), textIp);
            });

            NetworkManager.ResetConnection();
            serverTask.Start();

            while (!NetworkManager.Exit)
            {
                if (NetworkManager.Server != null) //Wait for the server to accept
                {
                    ChatWindow chatWindow = new ChatWindow(textName, NetworkManager);
                    chatWindow.DataContext = new ChatWindowViewModel(NetworkManager);
                    chatWindow.ShowDialog();

                    break;
                }
            }
        }
       
        public void JoinChatServer()
        {
            //Start the server on a separate thread
            Thread clientTask = new Thread(() =>
            {
                NetworkManager.JoinConnection(textName, int.Parse(textPort), textIp);
            });

            NetworkManager.ResetConnection();
            clientTask.Start();
            while (!NetworkManager.Exit)
            {
                if (NetworkManager.Client != null) //Wait for the server to accept
                {
                    ChatWindow chatWindow = new ChatWindow(textName, NetworkManager);
                    chatWindow.DataContext = new ChatWindowViewModel(NetworkManager);
                    chatWindow.ShowDialog();

                    break;
                }
            }
        }

        private void NetworkManager_PromptAcceptClient(Object sender, TcpClient client)
        {
            bool? result = false;

            Application.Current.Dispatcher.Invoke(() =>
            {
                //Create and display the custom dialog window here.
                AcceptRejectDialog dialog = new AcceptRejectDialog();

                //Show the dialog and wait for the user's response.
                result = dialog.ShowDialog();
            });

            if (result.HasValue && result.Value)
            {
                //The user clicked "Accept."
                NetworkManager.Accepted = true;
            }
            else
            {
                //The user clicked "Reject" or closed the dialog.
                NetworkManager.Accepted = false;
            }
        }
    }
}