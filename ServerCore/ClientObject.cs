using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace ServerCore
{
    class ClientObject
    {
        TcpClient client;
        ICommands commands;
        DateTime timeStart;

        public ClientObject(TcpClient client, ICommands commands)
        {
            this.client = client;
            this.commands = commands;
            timeStart = DateTime.Now;

            Console.WriteLine("Connect " + client.Client.RemoteEndPoint.ToString());
        }

        public bool NeedUpdate
        {
            get
            {
                return client.GetStream().DataAvailable;
            }
        }

        public bool IsEnable
        {
            get
            {
                return (DateTime.Now - timeStart).TotalSeconds < 10;
            }
        }

        public string Process(int delay)
        {
            string result = "some client been a handled";
            NetworkStream stream = null;
            try
            {
                stream = client.GetStream();
                byte[] data = new byte[64];

                StringBuilder builder = new StringBuilder();
                int bytes = 0;
                while (stream.DataAvailable)
                {
                    bytes = stream.Read(data, 0, data.Length);
                    builder.Append(Encoding.Unicode.GetString(data, 0, bytes));
                }

                string message = builder.ToString();
                string[] query = message.Split(new char[] { '|' });

                string[] queryArgs = null;
                if (query.Length > 1)
                {
                    queryArgs = new string[query.Length - 1];
                    for (int i = 0; i < queryArgs.Length; ++i)
                        queryArgs[i] = query[i + 1];
                }

                if (commands != null)
                {
                    try
                    {
                        message = commands.ExecuteCommand(query[0], queryArgs);
                    }
                    catch
                    {
                        message = "crash commands";
                    }
                }
                else
                    message = "c0";

                result = "\n----------------------------------------\nCLIENT-T-" + Thread.CurrentThread.ManagedThreadId.ToString() + " REQUEST: " + query[0] + " RESPONSE: " + message + "\n----------------------------------------\n";

                byte[] response = Encoding.Unicode.GetBytes(message);

                DateTime start = DateTime.Now;
                bool isSended = false;
                while (!isSended && (DateTime.Now - start).TotalMilliseconds < delay)
                {
                    try
                    {
                        client.GetStream().Write(response, 0, response.Length);
                        isSended = true;
                    }
                    catch (SocketException)
                    {
                        Thread.Sleep(100);
                    }
                }
                if (!isSended)
                    result = "CLIENT-T-" + Thread.CurrentThread.ManagedThreadId.ToString() + " didn't respond";
            }
            finally
            {
                //if (stream != null)
                //    stream.Close();
                //if (client != null)
                //    client.Close();
            }
            return result;
        }

        public void CloseConnection()
        {
            Console.WriteLine("Connect Close " + client.Client.RemoteEndPoint.ToString());

            try
            {
                byte[] response = Encoding.Unicode.GetBytes("bb");
                client.GetStream().Write(response, 0, response.Length);
            }
            catch (IOException)
            {
                // client closed the connection
            }

            client.Dispose();
        }
    }
}
