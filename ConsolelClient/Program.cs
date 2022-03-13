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

            int i = 2;
            bool isJob = true;
            while (isJob)
            {
                if (gettedRequest)
                {
                    //Console.Write("Message: ");
                    //string message = Console.ReadLine();
                    //if (message.ToLower() == "stop send")
                    //    isJob = false;
                    //else
                    //{
                    Console.WriteLine("--- " + i.ToString());
                        client.Push(i.ToString());
                        gettedRequest = false;
                    //}
                    ++i;
                }
                else
                {
                    Thread.Sleep(10);
                }
            }
        }
    }
}
