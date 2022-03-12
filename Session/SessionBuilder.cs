using System;
using System.Linq;
using System.Net;

namespace SessionLib
{
    public class SessionBuilder
    {
        private ISessionList allSessions;

        public SessionBuilder(ISessionList sessions)
        {
            allSessions = sessions;
        }

        public Session Create(long id, IPAddress ip)
        {
            if(id > 0)
            {
                var curSession = allSessions.Where(x => x.Id == id);
                if(curSession.Count() > 0)
                {
                    Console.WriteLine("Сontinuation of the session: " + id.ToString());
                    return curSession.First();
                }
            }

            long uniqueId = long.Parse(DateTime.Now.ToString("yyyyMMddHHmmssfff"));
            Session newSession = new Session(uniqueId, ip);
            allSessions.Add(newSession);
            Console.WriteLine("New session: " + newSession.Id.ToString());
            return newSession;
        }
    }
}
