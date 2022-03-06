using System;
using System.IO;
using System.Net.Sockets;
using System.Text;

namespace ServerCore
{
    class ClientObject
    {
        private readonly TcpClient client;
        private readonly ICommands commands;
        private readonly DateTime timeStart;
        private bool connected;

        public ClientObject(TcpClient client, ICommands commands)
        {
            this.client = client;
            this.commands = commands;
            timeStart = DateTime.Now;
            connected = true;
        }

        public bool HaveMessage => client.GetStream().DataAvailable;

        public bool IsEnable => connected && (DateTime.Now - timeStart).TotalSeconds < ServerConfig.ClientSecondsLifetime;

        public void Update()
        {
            NetworkStream stream = client.GetStream();
            string message = ReadData(stream);
            string response;

            if (commands != null)
            {
                ParseMessage(message, out string command, out string[] args);

                try
                {
                    response = commands.ExecuteCommand(command, args);
                }
                catch (Exception e)
                {
                    response = "Commands Fail:\n" + e.ToString();
                }
            }
            else
                response = "Commands is null";

            SendResponse(response);

            Console.WriteLine("----------------------------------------");
            Console.WriteLine("REQUEST: " + message);
            Console.WriteLine("RESPONSE: " + response);
            Console.WriteLine("----------------------------------------");
        }

        public void CloseConnection()
        {
            Console.WriteLine("Connect Close " + client.Client.RemoteEndPoint.ToString());

            SendResponse(ServerConfig.CloseConnectionCommand);
            client.Dispose();
        }

        private string ReadData(NetworkStream stream)
        {
            byte[] data = new byte[ServerConfig.DataReadPacketSize];
            StringBuilder builder = new StringBuilder();
            while (stream.DataAvailable)
            {
                int bytes = stream.Read(data, 0, data.Length);
                builder.Append(Encoding.Unicode.GetString(data, 0, bytes));
            }

            return builder.ToString();
        }

        private void ParseMessage(string message, out string command, out string[] args)
        {
            string[] query = message.Split(ServerConfig.CommandArgsSplitter);

            args = null;
            if (query.Length > 1)
            {
                args = new string[query.Length - 1];
                for (int i = 0; i < args.Length; ++i)
                    args[i] = query[i + 1];

                command = query[0];
            }
            else if (query.Length > 0)
                command = query[0];
            else
                command = message;
        }

        private void SendResponse(string response)
        {
            try
            {
                byte[] responseBytes = Encoding.Unicode.GetBytes(response);
                client.GetStream().Write(responseBytes, 0, responseBytes.Length);
            }
            catch (IOException)
            {
                connected = false; // client closed the connection
            }
        }
    }
}
