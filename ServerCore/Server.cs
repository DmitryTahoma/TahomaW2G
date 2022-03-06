using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace ServerCore
{
    public delegate void MessageAction(string message);

    public class Server
    {
        event MessageAction OnMessageAdded;

        TcpListener listener;
        ICommands commands;
        bool listen;

        IPAddress ip;
        int port;

        List<ClientObject> clients;
        List<ClientObject> newClients;
        List<ClientObject> disconnectedClients;
        object newClientsLocker;

        public bool IsStarted { private set; get; }

        public Server(ICommands commands, IPAddress ip, int port)
        {
            this.commands = commands;
            this.ip = ip;
            this.port = port;

            clients = new List<ClientObject>();
            newClients = new List<ClientObject>();
            disconnectedClients = new List<ClientObject>();
            newClientsLocker = new object();
        }

        public Server(ICommands commands, string ip, int port) : this(commands, IPAddress.Parse(ip), port) { }

        public void Listenning()
        {
            if (IsStarted)
                return;

            if (commands == null)
                OnMessageAdded?.Invoke("Commands slot is void");
            try
            {
                listener = new TcpListener(ip, port);
                listener.Start();
                OnMessageAdded?.Invoke("Server started " + ip.ToString() + ":" + port.ToString());

                listen = true;
                IsStarted = true;
                while (listen)
                {
                    TcpClient client = null;
                    try
                    {
                        client = listener.AcceptTcpClient();
                        lock (newClientsLocker)
                        {
                            newClients.Add(new ClientObject(client, commands));
                        }
                        //Thread clientThread = new Thread(new ThreadStart(() =>
                        //{
                        //    OnMessageAdded?.Invoke(clientObject.Process(1000));
                        //}));
                        //clientThread.Start();
                    }
                    catch (SocketException) { }
                }
            }
            finally
            {
                if (listener != null)
                    listener.Stop();
            }
        }

        public void Update()
        {
            foreach (ClientObject client in clients)
            {
                if (client.NeedUpdate)
                {
                    OnMessageAdded?.Invoke(client.Process(1000));
                }
            }

            AddNewClients();
            RemoveDisconnectedClients();
        }

        private void AddNewClients()
        {
            lock (newClientsLocker)
            {
                clients.AddRange(newClients);
                newClients.Clear();
            }
        }

        private void RemoveDisconnectedClients()
        {
            foreach (ClientObject client in clients)
            {
                if (!client.IsEnable)
                {
                    disconnectedClients.Add(client);
                }
            }
            foreach (ClientObject client in disconnectedClients)
            {
                clients.Remove(client);
                client.CloseConnection();
            }
            disconnectedClients.Clear();
        }

        public void Stop()
        {
            IsStarted = false;
            listen = false;
            Thread.Sleep(1000);
            if (listener != null)
                listener.Stop();
        }

        public void AddMessageHandle(MessageAction action)
        {
            OnMessageAdded += action;
        }
    }
}
