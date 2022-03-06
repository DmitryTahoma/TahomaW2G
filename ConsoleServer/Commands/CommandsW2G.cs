using ServerCore;

namespace ConsoleServer.Commands
{
    class CommandsW2G : ICommands
    {
        public string ExecuteCommand(string commandName, string[] args)
        {
            if(int.TryParse(commandName, out int num))
            {
                return (num * num).ToString();
            }

            return "not a int number";
        }
    }
}
