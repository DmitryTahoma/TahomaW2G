using ClientCore;
using System;

namespace ConsolelClient
{
    class Program
    {
        static void Main(string[] args)
        {
            ClientAsync client = new ClientAsync("192.168.0.168", 4210);
            client.ConnectAsync();
            client.StartListenningResponseAsync();
            client.OnGettingMessage += (response) => {
                Console.WriteLine(response);
            };

            bool isJob = true;
            while (isJob)
            {
                string message = Console.ReadLine();
                if (message.ToLower() == "stop send")
                    isJob = false;
                else
                    client.PushAsync(message);
            }
        }
    }
}
