using ServerCore;

namespace ConsoleServer.Commands
{
    class CommandsW2G : ICommands
    {
        public string ExecuteCommand(string message)
        {
            if(int.TryParse(message, out int num))
            {
                return (num * num).ToString();
            }

            return "not a int number";
        }
    }
}
