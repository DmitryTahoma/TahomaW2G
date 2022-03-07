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

        private string streamString;

        public ClientObject(TcpClient client, ICommands commands)
        {
            this.client = client;
            this.commands = commands;
            timeStart = DateTime.Now;
            connected = true;

            streamString = string.Empty;
        }

        public bool HaveMessage => client.GetStream().DataAvailable || HaveMessageInStreamString();

        public bool IsEnable => connected && ((DateTime.Now - timeStart).TotalSeconds < ServerConfig.ClientSecondsLifetime || HaveMessage);

        public void Update()
        {
            NetworkStream stream = client.GetStream();
            ReadData(stream);
            string message = GetMessage();

            if(string.IsNullOrEmpty(message))
            {
                return;
            }

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

        private void ReadData(NetworkStream stream)
        {
            byte[] data = new byte[ServerConfig.DataReadPacketSize];
            StringBuilder builder = new StringBuilder();
            while (stream.DataAvailable)
            {
                int bytes = stream.Read(data, 0, data.Length);
                builder.Append(Encoding.Unicode.GetString(data, 0, bytes));
            }

            streamString += builder.ToString();
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
                byte[] responseBytes = Encoding.Unicode.GetBytes(ServerConfig.StartMessagePart + response + ServerConfig.EndMessagePart);
                client.GetStream().Write(responseBytes, 0, responseBytes.Length);
            }
            catch (IOException)
            {
                connected = false; // client closed the connection
            }
            catch (InvalidOperationException)
            {
                connected = false; // client closed the connection
            }
        }

        private string GetMessage()
        {
            int indexStart = streamString.IndexOf(ServerConfig.StartMessagePart);
            int indexEnd = streamString.IndexOf(ServerConfig.EndMessagePart);

            if (indexStart >= 0 && indexEnd >= 0)
            {
                string res = streamString.Substring(indexStart + ServerConfig.StartMessagePart.Length, indexEnd - ServerConfig.EndMessagePart.Length + 1);

                int newStart = indexEnd + ServerConfig.EndMessagePart.Length;
                streamString = streamString.Substring(newStart, streamString.Length - newStart);

                return res;
            }
            else
            {
                return string.Empty;
            }
        }

        private bool HaveMessageInStreamString()
        {
            return streamString.IndexOf(ServerConfig.StartMessagePart) >= 0 && streamString.IndexOf(ServerConfig.EndMessagePart) >= 0;
        }
    }
}
