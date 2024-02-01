using MyDemo.Model;
using System.Windows;
using MyDemo.ViewModel.Command;
using System;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading;
using System.Windows.Input;
using System.ComponentModel;

namespace MyDemo.ViewModel
{
    internal class ChatWindowViewModel : INotifyPropertyChanged
    {
        private ICommand _sendTextMessage;
        private ICommand _searchForConversation;
        private ICommand _sendBuzz;
        private ICommand _closeWindow;

        private ObservableCollection<Conversation> conversationStorage;
        private Conversation conversation;
        private Conversation selectedConversation;


        private string _conversationSearch;
        private string textMessage;
        private string textName;
        public string textInfo;


        public event PropertyChangedEventHandler PropertyChanged;
        public ObservableCollection<Message> Messages { get; private set; }
        private NetworkManager NetworkManager { get; set; }

        public ChatWindowViewModel(NetworkManager networkManager)
        {
            
            NetworkManager = networkManager;
            NetworkManager.MessageReceived += NetworkManager_MessageReceived;
            NetworkManager.ClientConnected += NetworkManager_ClientConnected;
            NetworkManager.ClientDisconnected += NetworkManager_ClientDisconnected;
            NetworkManager.Disconnect += NetworkManager_Disconnect;

            LoadConversations("");
            Messages = new ObservableCollection<Message>();

            if(NetworkManager.Client != null)
            {
                if (NetworkManager.Client.Connected)
                {
                    SendInfo($"Conversing with {NetworkManager.ConnectedName}");
                }
            }
            

        }

        public ObservableCollection<Conversation> ConversationStorage
        {
            get { return conversationStorage; }
            set
            {
                conversationStorage = value;
                OnPropertyChanged(nameof(ConversationStorage));
            }
        }

        private Conversation Conversation {
            get { return conversation; }
            set
            {
                conversation = value;
            }
        }

        public string TextMessage
        {
            get { return textMessage; }
            set
            {
                textMessage = value;
            }
        }

        public string conversationSearch
        {
            get { return _conversationSearch; }
            set
            {
                _conversationSearch = value;
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
        public string TextName
        {
            get { return textName; }
            set
            {
                textName = value;
            }
        }

        public ICommand sendMessage
        {
            get
            {
                if (_sendTextMessage == null)
                    _sendTextMessage = new SendMessageCommand(this);
                return _sendTextMessage;
            }
            set
            {
                _sendTextMessage = value;
            }
        }

        public ICommand searchForConversation
        {
            get
            {
                if (_searchForConversation == null)
                    _searchForConversation = new SearchForConversationCommand(this);
                return _searchForConversation;

            }
            set
            {
                _searchForConversation = value;
            }
        }

        public ICommand sendBuzz
        {
            get
            {
                if (_sendBuzz == null)
                    _sendBuzz = new SendBuzzCommand(this);
                return _sendBuzz;
            }
            set
            {
                _sendBuzz = value;
            }
        }

        public ICommand closeWindow
        {
            get
            {
                if (_closeWindow == null)
                {
                    _closeWindow = new RelayCommand(param => this.CloseWindow(), null);
                }
                return _closeWindow;
            }
        }

        public Conversation SelectedConversation
        {
            get { return selectedConversation; }
            set
            {
                selectedConversation = value;
                if (selectedConversation != null)
                {
                    //If the client is connected to the server, there will be a Yes or No question, if not there will be no question
                    if (NetworkManager.Client != null)
                    {
                        if(NetworkManager.Client.Connected)
                        {
                        MessageBoxResult result = MessageBox.Show("Are you sure you want to leave the current conversation? The connection will be lost.", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);
                        if (result == MessageBoxResult.Yes) 
                        {
                            //Exits the connection and displays the selected conversation
                            string newMessage = $"EXIT";
                            byte[] Message = Encoding.ASCII.GetBytes(newMessage);
                            NetworkManager.NetworkStream.Write(Message, 0, Message.Length);

                            Messages = new ObservableCollection<Message>(selectedConversation.Messages);
                            OnPropertyChanged(nameof(Messages));
                        }
                        }
                        else
                        {
                            //Displays the selected conversation
                            Messages = new ObservableCollection<Message>(selectedConversation.Messages);
                            OnPropertyChanged(nameof(Messages));
                        }
                    }
                    else
                    {
                        //Displays the selected conversation
                        Messages = new ObservableCollection<Message>(selectedConversation.Messages);
                        OnPropertyChanged(nameof(Messages));
                    }
                }
            }
        }


        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void NetworkManager_MessageReceived(object sender, Message message)
        {
            //Subscribe event when a message is recieved
            OnMessageReceived(message);
        }

        private void NetworkManager_ClientConnected(object sender, string name)
        {
            //Subscribe event when a connection is made
            SelectedConversation = null;


            string info = $"Conversing with {name}";
            SendInfo(info);

            Messages = new ObservableCollection<Message>();
            OnPropertyChanged(nameof(Messages));
        }

        private void NetworkManager_ClientDisconnected(object sender, string name)
        {
            //Subscribe event when a disconnect
            var conversationManager = new ConversationManager("../../../Model/Conversations.json");

            // Create a new conversation
            Conversation newConversation = new Conversation
            {
                Member1 = name,
                Member2 = NetworkManager.Name,
                Messages = Messages,
                Time = DateTime.Now

            };


            // Save the new conversation
            conversationManager.SaveConversation(newConversation);
            LoadConversations("");
            string info = $"{name} has disconnected";
            SendInfo(info);

        }
        private void NetworkManager_Disconnect(object sender, string throwAway)
        {
            //Subscribe event when you disconnect
            NetworkManager.ClientDisconnected -= NetworkManager_ClientDisconnected;

            LoadConversations("");
            string info = $"You have disconnected";
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

        public void CloseWindow()
        {
            //Before closing the program down we want to send the other user a message that we are exiting
            if(NetworkManager.Client != null)
            {
                if (NetworkManager.Client.Connected)
                {
                    string newMessage = $"EXIT";
                    byte[] Message = Encoding.ASCII.GetBytes(newMessage);
                    NetworkManager.NetworkStream.Write(Message, 0, Message.Length);
                }
            }
            if (NetworkManager.Owner)
            {
                NetworkManager.StopServer();
            }
        }

        private void LoadConversations(string search)
        {
            //Function for loading in the old conversations (the string search is used if we the user had a specific search string)
            var conversationManager = new ConversationManager("../../../Model/Conversations.json");

            // Load existing conversations
            ObservableCollection<Conversation> existingConversations = conversationManager.LoadConversations(NetworkManager.Name, search);

            
            foreach (var conversation in existingConversations)
            {
                //Sets the PreviewInfo to get the 
                conversation.DynamicPreviewInfo = conversation.GetPreviewInfo(NetworkManager.Name);
            }

            // Set the ConversationStorage property with the loaded conversations
            ConversationStorage = existingConversations;
        }

        private void OnMessageReceived(Message message)
        {
            //Adds the message to the ObservableCollection Messages and Ivoke it to update the UI. 
            Application.Current.Dispatcher.Invoke(() =>
            {
                Messages.Add(message);
            });
        }

        public void SendMessage()
        {
            //Create the message
            Message message = new Message
            {
                MessageSender = NetworkManager.Name,
                PublishDate = DateTime.Now,
                MessageContent = textMessage
            };

            if (NetworkManager.Client != null)
            {
                if (NetworkManager.Client.Connected)
                {
                    //Sends the message
                    Thread sendMessage = new Thread(() =>
                    {
                        NetworkManager.SendMessage(message);
                    });

                sendMessage.Start();
                //Calls OnMessageRecieved since we do not listen to our own message on the stream
                OnMessageReceived(message);
                }
                else
                {
                    //Sends a feedback message that no one is connected
                    string info = "Who are you talking to? There is no one connected!";
                    SendInfo(info);
                }
            }
            else
            {
                //Sends a feedback message that no one is connected
                string info = "Who are you talking to? There is no one connected!";
                SendInfo(info);
            }
        }

        public void ConversationSearch()
        {
            LoadConversations(conversationSearch);
        }

        public void SendBuzz()
        {
            if(NetworkManager.Client != null)
            {
                if (NetworkManager.Client.Connected)
                {

                    Thread sendBuzz = new Thread(() =>
                    {
                        NetworkManager.SendBuzz();
                    });

                    sendBuzz.Start();
                
                }
                else
                {
                    string info = "Who are you Buzzing to? There is no one connected!";
                    SendInfo(info);
                }
            }
            else
            {
                string info = "Who are you Buzzing to? There is no one connected!";
                SendInfo(info);
            }
        }
    }
}

