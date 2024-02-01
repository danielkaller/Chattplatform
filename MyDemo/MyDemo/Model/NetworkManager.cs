using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Threading;
using MyDemo.View;
using System.Windows;
using System.Text.Json;
using System.Collections.ObjectModel;



namespace MyDemo.Model
{
    public class NetworkManager
    {
        private bool accepted;
        private bool exit;
        private bool owner;

        private int port;

        private string connectedName;
        private string ip;
        private string name;

        private TcpClient client;
        private TcpListener server;
        private NetworkStream networkStream;

        public event EventHandler<Message> MessageReceived;
        public event EventHandler<TcpClient> PromptAcceptClient;
        public event EventHandler<string> ClientConnected;
        public event EventHandler<string> ClientDisconnected;
        public event EventHandler<string> Disconnect;
        public event EventHandler<string> RequestDeclined;
        public event EventHandler<string> RequestWait;
        public event EventHandler<string> ServerDown;
        public event EventHandler<string> ServerOccupied;


        public bool Accepted
        {
            get { return accepted; }
            set { accepted = value; }
        }

        public bool Exit
        {
            get { return exit; }
            set { exit = value; }
        }

        public bool Owner
        {
            get { return owner; }
            set { owner = value; }
        }

        public int Port
        {
            get { return port; }
            set { port = value; }
        }

        public string ConnectedName
        {
            get { return connectedName; }
            set { connectedName = value; }
        }

        public string Ip
        {
            get { return ip; }
            set { ip = value; }
        }

        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        public TcpClient Client
        {
            get { return client; }
            set { client = value; }
        }

        public TcpListener Server
        {
            get { return server; }
            set { server = value; }
        }

        public NetworkStream NetworkStream
        {
            get { return networkStream; }
            set { networkStream = value; }
        }

        private void OnMessageReceived(Message message)
        {
            MessageReceived?.Invoke(this, message);
        }

        private void OnPromptAcceptClient(TcpClient client)
        {
            PromptAcceptClient?.Invoke(this, client);
        }

        private void OnClientConnected(string name)
        {
            ClientConnected?.Invoke(this, name);
        }

        private void OnClientDisconnected(string name)
        {
            ClientDisconnected?.Invoke(this, name);
        }

        private void OnDisconnect(string throwAway)
        {
            Disconnect?.Invoke(this, "");
        }

        private void OnRequestDeclined(string name)
        {
            RequestDeclined?.Invoke(this, name);
        }

        private void OnRequestWait(string throwAway)
        {
            RequestWait?.Invoke(this, "");
        }
        private void OnServerOccupied(string throwAway)
        {
            ServerOccupied?.Invoke(this, "");
        }

        private void OnServerDown(string throwAway)
        {
            ServerDown?.Invoke(this, "");
        }

        public void ResetConnection()
        {
            Client = null;
            Exit = false;
            Accepted = false;
            ConnectedName = null;
            NetworkStream = null;
            Owner = false;
            Server = null;
        }

        public bool StartConnection(string Name, int Port, string Ip)
        {
            Owner = true;

            if (CheckServer(Port, Ip))
            {

                this.Exit = true;
                OnServerOccupied("");

                return false; // Server is already running
            }
            else
            {
                this.Port = Port;
                this.Ip = Ip;
                this.Name = Name;
            }

            try
            {

                // Set the TcpListener on port 13000.
                Int32 port = Port;
                IPAddress localAddr = IPAddress.Parse(Ip);


                Server = new TcpListener(localAddr, port);

                // Start listening for client requests.
                Server.Start();

                // Buffer for reading data
                Byte[] bytes = new Byte[256];
                String data = null;


                while (true)
                {
                    TcpClient client = server.AcceptTcpClient();
                    try
                    {
                        HandleClient(client);
                    }
                    catch (Exception ex)
                    {
                        client?.Close();
                    }
                }
            }
            catch (SocketException e)
            {
                System.Diagnostics.Debug.WriteLine("SocketException: {0}", e);
            }
            finally
            {
                System.Diagnostics.Debug.WriteLine("Server Stopped");
                Server.Stop();
            }
            System.Diagnostics.Debug.WriteLine("\nHit enter to continue...");
            Console.Read();
            return true;
        }

        public void StopServer()
        {
            try
            {
                if (Server != null && Server.Server.IsBound)
                {
                    Server.Stop();
                    System.Diagnostics.Debug.WriteLine("Server Stopped");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("Server is not running.");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Exception while stopping server: " + ex.Message);
            }
        }

        public async void JoinConnection(string Name, int Port, string Ip)
        {

            try
            {
                TcpClient client = new TcpClient();
                if (CheckServer(Port, Ip))
                {
                    this.Owner = false;
                    this.Port = Port;
                    this.Ip = Ip;
                    this.Name = Name;
                    await client.ConnectAsync(Ip, Port);
                    // Send a join request message to the server
                    NetworkStream = client.GetStream();
                    byte[] requestMessage = Encoding.ASCII.GetBytes($"JOIN_REQUEST:{Name}");
                    NetworkStream.Write(requestMessage, 0, requestMessage.Length);
                    Listener(client);
                }
                else
                {
                    this.Exit = true;
                    OnServerDown("");
                }
            }
            catch (SocketException e)
            {
                System.Diagnostics.Debug.WriteLine("Failed to connect to the server: " + e.Message);
                // Failed to join the server
            }
        }



        public bool CheckServer(int Port, string Ip)
        {
            try
            {
                using (TcpClient client = new TcpClient())
                {
                    client.Connect(Ip, Port);
                    client.Close();
                    return true;
                }
            }
            catch (SocketException)
            {
                return false; // Server is not reachable or there's an issue
            }

        }

        private async Task Listener(TcpClient tcpClient)
        {
            try
            {
                byte[] buffer = new byte[1024];
                while (tcpClient.Connected)
                {
                    int bytesRead = await NetworkStream.ReadAsync(buffer, 0, buffer.Length);

                    if (bytesRead == 0)
                    {
                        break;
                    }

                    string newMessage = Encoding.ASCII.GetString(buffer, 0, bytesRead);

                    if (newMessage.StartsWith("JOIN_ACCEPTED"))
                    {
                        string[] parts = newMessage.Split(':');

                        string NameOfServer = parts[1];
                        this.ConnectedName = NameOfServer;

                        Client = tcpClient;

                        // Successfully joined the server
                    }
                    else if (newMessage.StartsWith("JOIN_DECLINED"))
                    {
                        string[] parts = newMessage.Split(':');

                        string NameOfServer = parts[1];
                        this.Exit = true;

                        OnRequestDeclined(NameOfServer);

                        tcpClient.Close();
                        break;
                    }
                    else if (newMessage.StartsWith("MESSAGE"))
                    {
                        System.Diagnostics.Debug.WriteLine("IN MESSAGE: ");
                        string[] parts = newMessage.Split('_');
                        string jsonString = parts[1];
                        Message? message = JsonSerializer.Deserialize<Message>(jsonString);

                        OnMessageReceived(message);

                    }
                    else if (newMessage.StartsWith("BUZZ"))
                    {
                        System.Diagnostics.Debug.WriteLine($"BUZZ");
                        var soundPlayer = new System.Media.SoundPlayer("../../../Model/21Savage.wav");
                        soundPlayer.Play();
                    }
                    else if (newMessage.StartsWith("AFFIRMATIVE"))
                    {
                        OnDisconnect("");
                    }
                    else if (newMessage.StartsWith("EXIT"))
                    {
                        OnClientDisconnected(ConnectedName);
                        string responseMessage = $"AFFIRMATIVE:{this.Name}";
                        byte[] responseBuffer = Encoding.ASCII.GetBytes(responseMessage);
                        if (Client.Connected)
                        {
                            networkStream.Write(responseBuffer, 0, responseBuffer.Length);
                            this.Client.Close();
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                // Handle any exceptions that may occur during communication with the client.
                System.Diagnostics.Debug.WriteLine("214 Exception: " + ex.Message);
            }
            finally
            {
                // Close the client to release resources
                tcpClient.Close();
            }
        }


        private async Task HandleClient(TcpClient tcpClient)
        {
            Client = tcpClient;
            networkStream = tcpClient.GetStream();
            try
            {
                byte[] buffer = new byte[1024];
                while (tcpClient.Connected)
                {
                    int bytesRead = await networkStream.ReadAsync(buffer, 0, buffer.Length);
                    if (bytesRead == 0)
                    {
                        break;
                    }

                    string newMessage = Encoding.ASCII.GetString(buffer, 0, bytesRead);

                    string NameOfClient;

                    if (newMessage.StartsWith("JOIN_REQUEST"))
                    {
                        string[] parts = newMessage.Split(':');

                        NameOfClient = parts[1];
                        this.ConnectedName = NameOfClient;

                        OnPromptAcceptClient(tcpClient);

                        if (this.Accepted)
                        {

                            string responseMessage = $"JOIN_ACCEPTED:{this.Name}";
                            byte[] responseBuffer = Encoding.ASCII.GetBytes(responseMessage);
                            networkStream.Write(responseBuffer, 0, responseBuffer.Length);
                            OnClientConnected(NameOfClient);

                        }
                        else
                        {
                            string responseMessage = $"JOIN_DECLINED:{this.Name}";
                            byte[] responseBuffer = Encoding.ASCII.GetBytes(responseMessage);
                            networkStream.Write(responseBuffer, 0, responseBuffer.Length);
                            tcpClient.Close();
                            return;
                        }

                    }
                    else if (newMessage.StartsWith("MESSAGE"))
                    {
                        System.Diagnostics.Debug.WriteLine("IN MESSAGE: ");
                        string[] parts = newMessage.Split('_');
                        string jsonString = parts[1];
                        Message? message = JsonSerializer.Deserialize<Message>(jsonString);
                        OnMessageReceived(message);

                    }
                    else if (newMessage.StartsWith("BUZZ"))
                    {
                        var soundPlayer = new System.Media.SoundPlayer("../../../Model/21Savage.wav");
                        soundPlayer.Play();
                    }
                    else if (newMessage.StartsWith("AFFIRMATIVE"))
                    {
                        StopServer();
                        OnDisconnect("");
                    }
                    else if (newMessage.StartsWith("EXIT"))
                    {
                        OnClientDisconnected(ConnectedName);
                        string responseMessage = $"AFFIRMATIVE:{this.Name}";
                        byte[] responseBuffer = Encoding.ASCII.GetBytes(responseMessage);
                        if (Client.Connected)
                        {
                            networkStream.Write(responseBuffer, 0, responseBuffer.Length);
                            this.Client.Close();
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("214 Exception: " + ex.Message);
            }
            finally
            {
                // Close the client to release resources
                tcpClient.Close();
            }
        }

        public void SendBuzz()
        {
            string newMessage = $"BUZZ:{this.Name}";
            byte[] Message = Encoding.ASCII.GetBytes(newMessage);
            networkStream.Write(Message, 0, Message.Length);
        }

        public void SendMessage(Message newMessage)
        {
            string jsonString = $"MESSAGE_{JsonSerializer.Serialize(newMessage)}";
            byte[] Message = Encoding.ASCII.GetBytes(jsonString);
            networkStream.Write(Message, 0, Message.Length);
        }
    }
}