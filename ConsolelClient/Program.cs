using ClientCore;
using System;
using System.IO;
using System.Threading;

namespace ConsolelClient
{
    class Program
    {
        static void Main(string[] args)
        {
            string username = string.Empty, sendTo = string.Empty, ip = string.Empty;
            long sessionId = 0;
            int port = -1;

            string path = "client_info.txt";
            if (File.Exists(path))
            {
                string info = string.Empty;
                using (StreamReader reader = new StreamReader(path)) 
                {
                    info = reader.ReadToEnd();
                }
                string[] data = info.Split('|');

                if(data.Length > 4)
                {
                    ip = data[0];
                    port = int.Parse(data[1]);
                    sessionId = long.Parse(data[2]);
                    username = data[3];
                    sendTo = data[4];
                }
            }

            ClientAsync client = new ClientAsync(ip, port, sessionId);
            client.ConnectAsync();
            client.StartListenningResponseAsync();

            if (string.IsNullOrEmpty(username))
            {
                Console.Write("Write your username: ");
                username = Console.ReadLine();
            }
            client.PushAsync("SetMyNick|" + username);

            if (string.IsNullOrEmpty(sendTo))
            {
                Console.Write("Write friend username: ");
                sendTo = Console.ReadLine();
            }

            Thread.Sleep(1000);
            using(StreamWriter writer = new StreamWriter(path))
            {
                writer.Write(ip + '|' + port.ToString() + '|' + client.GetSessionId().ToString() + '|' + username + '|' + sendTo);
            }
            Console.WriteLine("Started!");

            client.OnGettingMessage += (response) => {
                if(response.Contains("|"))
                {
                    string[] parts = response.Split('|');
                    if(parts.Length > 1)
                    {
                        Console.WriteLine(parts[0] + ": " + parts[1]);

                        return;
                    }
                }

                Console.WriteLine("UNKNOWN RESPONSE: " + response);
            };

            bool isJob = true;
            while (isJob)
            {
                string message = Console.ReadLine();
                if (message.ToLower() == "stop send")
                    isJob = false;
                else
                    client.PushAsync("SendMessageToUser|" + sendTo + "|" + message);
            }
        }
    }
}
