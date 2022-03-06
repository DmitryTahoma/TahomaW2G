using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace ClientCore
{
    public class Client
    {
        TcpClient client;

        IPAddress ip;
        int port;
        Thread listeningResponse;

        public Client(IPAddress ip, int port)
        {
            this.ip = ip;
            this.port = port;
            client = new TcpClient();

            listeningResponse = new Thread(() =>
            {
                while (client.Connected)
                {
                    string msg = "";
                    try
                    {
                        msg = ListeningResponse();

                        if(msg != "bb")
                        {
                            OnGettingMessage?.Invoke(msg);
                        }
                        else
                        {
                            Console.WriteLine("Reconnect");
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.ToString());
                    }
                }
            });
        }

        public Client(string ip, int port) : this(IPAddress.Parse(ip), port) { }

        public delegate void MessageHandler(string message);
        public event MessageHandler OnGettingMessage;

        public void Connect()
        {
            client.Connect(ip, port);
        }

        public void StartListenningResponse()
        {
            if (client.Connected)
            {
                listeningResponse.Start();
            }
        }

        public void Send(string message)
        {
            byte[] data = Encoding.Unicode.GetBytes(message);

            client.GetStream().Write(data, 0, data.Length);

            //return ListenResponse();
        }

        public void SendCommand(string command, string[] args)
        {
            if (args == null || args.Length == 0)
                Send(command);

            string message = command;

            for (int i = 0; i < args.Length; ++i)
                message += "|" + args[i];

            Send(message);
        }

        private string ListeningResponse()
        {
            NetworkStream stream = client.GetStream();

            //DateTime start = DateTime.Now;
            while (!stream.DataAvailable && client.Connected)
            {
                Thread.Sleep(10);
            }

            if (!client.Connected)
            {
                throw new Exception();
            }

            byte[] data = new byte[64];

            StringBuilder builder = new StringBuilder();
            int bytes = 0;
            do
            {
                bytes = stream.Read(data, 0, data.Length);
                builder.Append(Encoding.Unicode.GetString(data, 0, bytes));
            }
            while (stream.DataAvailable);

            string res = builder.ToString();

            if(res == "bb")
            {
                client.Close();
                client.Dispose();
                client = new TcpClient();

                Connect();
            }

            return res;
        }
    }
}