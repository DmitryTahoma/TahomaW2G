using ConsoleServer.Commands;
using ServerCore;
using System;
using System.Threading;

namespace ConsoleServer
{
    class Program
    {
        static void Main(string[] args)
        {
            Server server = new Server(new CommandsW2G(), "192.168.0.168", 4210);

            Thread listener = new Thread(() => { server.Listenning(); });
            listener.Start();

            while(true)
            {
                server.Update();
            }
        }
    }
}
