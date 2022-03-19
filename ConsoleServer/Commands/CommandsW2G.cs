using ServerCore;
using SessionLib;
using SessionLib.Messages;

namespace ConsoleServer.Commands
{
    class CommandsW2G : ICommands
    {
        public string ExecuteCommand(string message, ISessionList sessions, Session currentSession)
        {
            string command = message;
            string[] splitted = command.Split('|');

            if (command.Contains("|")) command = splitted[0];

            switch(command)
            {
                case "SetMyNick": if (splitted.Length > 0) {
                        currentSession.SessionInfo = splitted[1];
                        return "SetMyNick|good";
                    } break;
                case "SendMessageToUser": if (splitted.Length > 1) {
                        if (currentSession.SessionInfo != null)
                        {
                            bool isSended = false;
                            foreach (Session session in sessions)
                            {
                                if (session.SessionInfo?.ToString() == splitted[1])
                                {
                                    session.SendDataString = MessageFormatter.FormatFullMessage(currentSession.SessionInfo.ToString() + "|" + splitted[2]);
                                    isSended = true;
                                    break;
                                }
                            }

                            if (!isSended)
                                return "SendMessageToUser|user not found";
                            else
                                return "SendMessageToUser|good";
                        }
                        else
                            return "SendMessageToUser|I don't know you";
                    } break;
            }

            return "I don't know what to do with it";
        }
    }
}
