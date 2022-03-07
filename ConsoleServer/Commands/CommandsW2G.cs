using ServerCore;

namespace ConsoleServer.Commands
{
    class CommandsW2G : ICommands
    {
        public string ExecuteCommand(string commandName, string[] args)
        {
            System.Console.WriteLine("Start do something");
            System.Threading.Thread.Sleep(1000);
            System.Console.WriteLine("End do something");

            if(int.TryParse(commandName, out int num))
            {
                return (num * num).ToString();
            }

            return "not a int number";
        }
    }
}
