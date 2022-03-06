using ClientCore;
using System;
using System.Threading;

namespace ConsolelClient
{
    class Program
    {
        static void Main(string[] args)
        {
            Client client = new Client("192.168.0.168", 4210);
            bool gettedRequest = true;
            client.OnGettingMessage += (string response) => 
            {
                Console.WriteLine("Response:\n" + response + "\n");
                gettedRequest = true;
            };
            client.Connect();
            client.StartListenningResponse();

            bool isJob = true;
            while (isJob)
            {
                if (gettedRequest)
                {
                    Console.Write("Message: ");
                    string message = Console.ReadLine();
                    if (message.ToLower() == "stop send")
                        isJob = false;
                    else
                    {
                        client.Send(message);
                        gettedRequest = false;
                    }
                }
                else
                {
                    Thread.Sleep(10);
                }
            }
        }
    }
}
