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

        private Thread clientThread;
        private readonly object sendingDataLocker;

        public event MessageHandler OnGettingMessage;

        public Client(IPAddress ip, int port)
        {
            client = new TcpClient();
            this.ip = ip;
            this.port = port;
            session = new Session(0, (client.Client.RemoteEndPoint as IPEndPoint)?.Address);

            InitClientThread();
            sendingDataLocker = new object();

            Connected = false;
        }

        public Client(string ip, int port) : this(IPAddress.Parse(ip), port) { }

        public bool Connected { private set; get; }

        private void InitClientThread()
        {
            clientThread = new Thread(() =>
            {
                while (Connected)
                {
                    try
                    {
                        UpdateConnectionToServer();
                        if (!string.IsNullOrEmpty(session.SendDataString))
                            SendingData();

                        ListeningResponse();
                        if (HaveResponseInStreamString())
                        {
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
            });
        }

        public void Connect()
        {
            client.Connect(ip, port);
            Connected = true;
            PushFirst(session.Id.ToString());
        }

        public void StartListenningResponse()
        {
            if (!clientThread.IsAlive)
                clientThread.Start();
        }

        public void Push(string message)
        {
            lock (sendingDataLocker)
            {
                session.SendDataString += MessageTags.StartMessagePart + message + MessageTags.EndMessagePart;
            }
        }

        private void PushFirst(string message)
        {
            lock (sendingDataLocker)
            {
                session.SendDataString = MessageTags.StartMessagePart + message + MessageTags.EndMessagePart + session.SendDataString;
            }
        }

        private void Reconnect()
        {
            client.Dispose();
            Connected = false;
            session.Accepted = false;

            client = new TcpClient();
            Connect();
        }

        private bool TryReconnect()
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

        private void ListeningResponse()
        {
            NetworkStream stream = client.GetStream();

            byte[] data = new byte[ClientConfig.DataReadPacketSize];
            StringBuilder builder = new StringBuilder();
            while (stream.DataAvailable)
            {
                int bytes = stream.Read(data, 0, data.Length);
                builder.Append(Encoding.Unicode.GetString(data, 0, bytes));
            }

            session.StreamString += builder.ToString();
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
                WriteToClientStream(session.SendDataString);
                session.SendDataString = string.Empty;
            }
        }

        private void AcceptSession(string response)
        {
            if (long.TryParse(response, out long id))
            {
                session.Id = id;
                session.Accepted = true;
            }
        }

        private void UpdateConnectionToServer()
        {
            if (!Connected)
                while (!TryReconnect())
                    Thread.Sleep(ClientConfig.WaitServerConnectionDelay);
        }
    }
}