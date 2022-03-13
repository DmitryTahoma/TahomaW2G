using ClientCore;
using System;
using System.Threading;

namespace ConsolelClient
{
    class Program
    {
        static void Main(string[] args)
        {
            ClientAsync client = new ClientAsync("192.168.0.168", 4210);
            client.Connect();

            int i = 2;
            bool isJob = true;
            while (isJob)
            {
                //Console.Write("Message: ");
                //string message = Console.ReadLine();
                //if (message.ToLower() == "stop send")
                //    isJob = false;
                //else
                //{
                Console.WriteLine(i.ToString() + "^2 = " + client.Send(i.ToString()).Result);
                //}
                ++i;
            }
        }
    }
}
