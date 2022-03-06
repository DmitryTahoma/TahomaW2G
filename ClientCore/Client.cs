using System;
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

        private Thread listeningResponse;

        public event MessageHandler OnGettingMessage;

        public Client(IPAddress ip, int port)
        {
            client = new TcpClient();
            this.ip = ip;
            this.port = port;

            InitListeningThread();

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
                        string msg = ListeningResponse();

                        if (msg != ClientConfig.CloseConnectionCommand)
                            OnGettingMessage?.Invoke(msg);
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
        }

        public void Reconnect()
        {
            client.Dispose();
            Connected = false;

            client = new TcpClient();
            Connect();
        }

        public void StartListenningResponse()
        {
            if (Connected && !listeningResponse.IsAlive)
                listeningResponse.Start();
        }

        public void Send(string message)
        {
            byte[] data = Encoding.Unicode.GetBytes(message);
            client.GetStream().Write(data, 0, data.Length);
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

        private string ListeningResponse()
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

            string res = builder.ToString();
            ExecuteInnerCommands(res);
            return res;
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
    }
}