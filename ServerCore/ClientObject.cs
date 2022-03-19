using SessionLib;
using SessionLib.Messages;
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace ServerCore
{
    class ClientObject
    {
        private readonly TcpClient client;
        private readonly ICommands commands;
        private bool connected;

        private Session mySession;
        private readonly ISessionList allSessions;

        public ClientObject(TcpClient client, ICommands commands, ISessionList allSessions)
        {
            this.client = client;
            this.commands = commands;
            connected = true;

            mySession = new Session();
            this.allSessions = allSessions;

            LastActivity = DateTime.Now;
        }

        public bool HaveMessage => client.GetStream().DataAvailable || HaveMessageInStreamString();

        public bool HaveResponse => MessageFormatter.HaveTaggedMessage(mySession.SendDataString, MessageTags.StartMessagePart, MessageTags.EndMessagePart);

        public bool IsEnable => connected && ((DateTime.Now - LastActivity).TotalSeconds < ServerConfig.ClientSecondsLifetime || HaveMessage);

        public IPAddress Ip => (client.Client.RemoteEndPoint as IPEndPoint)?.Address;

        public DateTime LastActivity { get; private set; }

        public void Update()
        {
            NetworkStream stream = client.GetStream();
            ReadData(stream);
            string message = GetMessage();

            if(string.IsNullOrEmpty(message))
            {
                return;
            }

            LastActivity = DateTime.Now;
            string response;

            if (commands != null)
            {
                try
                {
                    response = commands.ExecuteCommand(message, allSessions, mySession);
                }
                catch (Exception e)
                {
                    response = "Commands Fail:\n" + e.ToString();
                }
            }
            else
                response = "Commands is null";

            SendResponse(MessageFormatter.FormatFullMessage(response));

            Console.WriteLine("----------------------------------------");
            Console.WriteLine("REQUEST: " + message);
            Console.WriteLine("RESPONSE: " + response);
            Console.WriteLine("----------------------------------------");
        }

        public void SendResponse()
        {
            if(SendResponse(mySession.SendDataString))
            {
                mySession.SendDataString = string.Empty;
            }
        }

        public void CloseConnection()
        {
            Console.WriteLine("Connect Close " + client.Client.RemoteEndPoint.ToString());

            SendResponse(MessageFormatter.FormatFullMessage(ServerConfig.CloseConnectionCommand));
            client.Dispose();
        }

        public long GetAcceptSessionId()
        {
            NetworkStream stream = client.GetStream();
            ReadData(stream);
            string message = GetMessage();

            if (long.TryParse(message, out long id))
                return id;

            return -1;
        }

        public void AcceptSession(Session session)
        {
            session.StreamString += mySession.StreamString;
            mySession = session;
            SendResponse(MessageFormatter.FormatFullMessage(mySession.Id.ToString()));
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

            mySession.StreamString += builder.ToString();
        }

        private bool SendResponse(string response)
        {
            try
            {
                byte[] responseBytes = Encoding.Unicode.GetBytes(response);
                client.GetStream().Write(responseBytes, 0, responseBytes.Length);
                return true;
            }
            catch (IOException)
            {
                connected = false; // client closed the connection
            }
            catch (InvalidOperationException)
            {
                connected = false; // client closed the connection
            }
            return false;
        }

        private string GetMessage()
        {
            if (MessageFormatter.HaveTaggedMessage(mySession.StreamString, MessageTags.StartMessagePart, MessageTags.EndMessagePart))
            {
                string res = MessageFormatter.SubstringFromTags(mySession.StreamString, MessageTags.StartMessagePart, MessageTags.EndMessagePart);
                mySession.StreamString = MessageFormatter.CutMessageFromTags(mySession.StreamString, MessageTags.StartMessagePart, MessageTags.EndMessagePart);
                return res;
            }
            else
            {
                return string.Empty;
            }
        }

        private bool HaveMessageInStreamString()
        {
            return MessageFormatter.HaveTaggedMessage(mySession.StreamString, MessageTags.StartMessagePart, MessageTags.EndMessagePart);
        }
    }
}
