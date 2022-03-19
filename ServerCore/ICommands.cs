using SessionLib;

namespace ServerCore
{
    public interface ICommands
    {
        string ExecuteCommand(string message, ISessionList sessions, Session currentSession);
    }
}
