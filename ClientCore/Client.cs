using SessionLib;
using SessionLib.Messages;
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace ClientCore
{
    public delegate void MessageHandler(string message);

    public class Client
    {
        private TcpClient client;
        private readonly IPAddress ip;
        private readonly int port;
        public Session session;

        private Thread listeningResponse;
        private Thread sendingRequest;
        private readonly object sendingDataLocker;

        public event MessageHandler OnGettingMessage;

        public Client(IPAddress ip, int port)
        {
            client = new TcpClient();
            this.ip = ip;
            this.port = port;
            session = new Session(0, (client.Client.RemoteEndPoint as IPEndPoint)?.Address);

            InitListeningThread();
            InitSendingThread();
            sendingDataLocker = new object();

            Connected = false;
        }

        public Client(string ip, int port) : this(IPAddress.Parse(ip), port) { }

        public bool Connected { private set; get; }

        private void InitListeningThread()
        {
            listeningResponse = new Thread(() =>
            {
                while (Connected)
                {
                    try
                    {
                        if (!HaveResponseInStreamString())
                        {
                            ListeningResponse();
                        }
                        string response = GetResponse();

                        if (!string.IsNullOrEmpty(response))
                        {
                            if (IsInnerCommand(response))
                                ExecuteInnerCommands(response);
                            else if (!session.Accepted)
                                AcceptSession(response);
                            else
                                OnGettingMessage?.Invoke(response);
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.ToString());
                    }
                }
            });
        }

        private void InitSendingThread()
        {
            sendingRequest = new Thread(() =>
            {
                while(!string.IsNullOrEmpty(session.SendDataString))
                {
                    if(!Connected)
                    {
                        while(!TryReconnect())
                        {
                            Thread.Sleep(ClientConfig.WaitServerConnectionDelay);
                        }

                        InitListeningThread();
                        StartListenningResponse();
                    }

                    SendingData();
                }
            });
        }

        public void Connect()
        {
            client.Connect(ip, port);
            Connected = true;
            SendFirst(session.Id.ToString());
        }

        public void Reconnect()
        {
            client.Dispose();
            Connected = false;
            session.Accepted = false;

            client = new TcpClient();
            Connect();
        }

        public bool TryReconnect()
        {
            try
            {
                Reconnect();
                return true;
            }
            catch
            {
                client.Dispose();
                Connected = false;
                return false;
            }
        }

        public void StartListenningResponse()
        {
            if (Connected && !listeningResponse.IsAlive)
                listeningResponse.Start();
        }

        public void Send(string message)
        {
            lock (sendingDataLocker)
            {
                session.SendDataString += MessageTags.StartMessagePart + message + MessageTags.EndMessagePart;
            }

            StartSending();
        }

        private void SendFirst(string message)
        {
            lock (sendingDataLocker)
            {
                session.SendDataString = MessageTags.StartMessagePart + message + MessageTags.EndMessagePart + message;
            }

            StartSending();
        }

        public void SendCommand(string command, string[] args)
        {
            if (args == null || args.Length == 0)
                Send(command);

            string message = command;

            for (int i = 0; i < args.Length; ++i)
                message += ClientConfig.CommandArgsSplitter + args[i];

            Send(message);
        }

        private void ListeningResponse()
        {
            NetworkStream stream = client.GetStream();
            WaitData(stream);

            byte[] data = new byte[ClientConfig.DataReadPacketSize];
            StringBuilder builder = new StringBuilder();
            do
            {
                int bytes = stream.Read(data, 0, data.Length);
                builder.Append(Encoding.Unicode.GetString(data, 0, bytes));
            }
            while (stream.DataAvailable);

            session.StreamString += builder.ToString();
        }

        private void WaitData(NetworkStream stream)
        {
            while (!stream.DataAvailable && Connected)
            {
                Thread.Sleep(ClientConfig.WaitDataDelay);
            }

            if (!Connected)
            {
                throw new Exception("Lost connection to the server");
            }
        }

        private void ExecuteInnerCommands(string command)
        {
            if (command == ClientConfig.CloseConnectionCommand)
                Reconnect();
        }

        private string GetResponse()
        {
            if (MessageFormatter.HaveTaggedMessage(session.StreamString, MessageTags.StartMessagePart, MessageTags.EndMessagePart))
            {
                string res = MessageFormatter.SubstringFromTags(session.StreamString, MessageTags.StartMessagePart, MessageTags.EndMessagePart);
                session.StreamString = MessageFormatter.CutMessageFromTags(session.StreamString, MessageTags.StartMessagePart, MessageTags.EndMessagePart);
                return res;
            }
            else
            {
                return string.Empty;
            }
        }

        private bool IsInnerCommand(string message)
        {
            return message == ClientConfig.CloseConnectionCommand;
        }

        private bool HaveResponseInStreamString()
        {
            return MessageFormatter.HaveTaggedMessage(session.StreamString, MessageTags.StartMessagePart, MessageTags.EndMessagePart);
        }

        private void WriteToClientStream(string message)
        {
            byte[] data = Encoding.Unicode.GetBytes(message);
            client.GetStream().Write(data, 0, data.Length);
        }

        private void SendingData()
        {
            lock (sendingDataLocker)
            {
                try
                {
                    WriteToClientStream(session.SendDataString);
                    session.SendDataString = string.Empty;
                }
                catch (IOException)
                {
                    Connected = false; // server closed the connection
                }
                catch (InvalidOperationException)
                {
                    Connected = false; // server closed the connection
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
            }
        }

        private void AcceptSession(string response)
        {
            if(long.TryParse(response, out long id))
            {
                session.Id = id;
                session.Accepted = true;
            }
        }

        private void StartSending()
        {
            if (!sendingRequest.IsAlive)
            {
                InitSendingThread();
                sendingRequest.Start();
            }
        }
    }
}