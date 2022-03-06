using ServerCore;

namespace ConsoleServer.Commands
{
    class CommandsW2G : ICommands
    {
        public string ExecuteCommand(string commandName, string[] args)
        {
            return "good";
        }
    }
}
