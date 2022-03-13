using SessionLib;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace ServerCore
{
    public class Server
    {
        private readonly TcpListener listener;
        private readonly IPAddress ip;
        private readonly int port;

        private readonly List<ClientObject> clients;
        private readonly List<ClientObject> newClients;
        private readonly List<ClientObject> disconnectedClients;
        private readonly object newClientsLocker;

        private readonly SessionBuilder sessionBuilder;
        private readonly ICommands commands;

        public Server(ICommands commands, IPAddress ip, int port, SessionBuilder sessionBuilder)
        {
            listener = new TcpListener(ip, port);
            this.ip = ip;
            this.port = port;

            clients = new List<ClientObject>();
            newClients = new List<ClientObject>();
            disconnectedClients = new List<ClientObject>();
            newClientsLocker = new object();

            this.sessionBuilder = sessionBuilder;
            this.commands = commands;

            IsStarted = false;
        }

        public Server(ICommands commands, string ip, int port, SessionBuilder sessionBuilder) : this(commands, IPAddress.Parse(ip), port, sessionBuilder) { }

        public bool IsStarted { private set; get; }

        public void Listenning()
        {
            if (IsStarted)
                return;

            if (commands == null)
                Console.WriteLine("Commands is null");
            try
            {
                listener.Start();
                IsStarted = true;

                Console.WriteLine("Server started " + ip.ToString() + ":" + port.ToString());

                while (IsStarted)
                {
                    try
                    {
                        TcpClient client = listener.AcceptTcpClient();
                        ClientObject clientObj = new ClientObject(client, commands);

                        lock (newClientsLocker)
                        {
                            newClients.Add(clientObj);
                        }

                        Console.WriteLine("Connect " + client.Client.RemoteEndPoint.ToString());
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Listenning exception\n" + e.ToString());
                    }
                }
            }
            finally
            {
                listener.Stop();
            }
        }

        public void Update()
        {
            foreach (ClientObject client in clients)
                if (client.HaveMessage)
                    client.Update();

            AddNewClients();
            RemoveDisconnectedClients();
        }

        private void AddNewClients()
        {
            List<ClientObject> acceptedClients = new List<ClientObject>();
            List<ClientObject> notAcceptedClients = new List<ClientObject>();

            lock (newClientsLocker)
            {
                DateTime now = DateTime.Now;

                foreach(ClientObject client in newClients)
                {
                    if (client.HaveMessage)
                    {
                        long id = client.GetAcceptSessionId();
                        if (id < 0)
                        {
                            notAcceptedClients.Add(client);
                        }
                        else
                        {
                            Session session = sessionBuilder.Create(id, client.Ip);
                            client.AcceptSession(session);
                            acceptedClients.Add(client);
                        }
                    }
                    else if ((now - client.LastActivity).TotalSeconds > ServerConfig.WaitAcceptClientSeconds)
                    {
                        notAcceptedClients.Add(client);
                    }
                }

                clients.AddRange(acceptedClients);
            }

            foreach(ClientObject client in notAcceptedClients)
            {
                client.CloseConnection();
            }
            notAcceptedClients.AddRange(acceptedClients);

            lock(newClientsLocker)
            {
                foreach(ClientObject client in notAcceptedClients)
                {
                    newClients.Remove(client);
                }
            }
        }

        private void RemoveDisconnectedClients()
        {
            foreach (ClientObject client in clients)
                if (!client.IsEnable)
                    disconnectedClients.Add(client);

            foreach (ClientObject client in disconnectedClients)
            {
                clients.Remove(client);
                client.CloseConnection();
            }

            disconnectedClients.Clear();
        }
    }
}
