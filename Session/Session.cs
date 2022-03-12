using System.Net;

namespace SessionLib
{
    public class Session
    {
        public Session()
        {
            Id = 0;
            Ip = null;
            Accepted = false;
            StreamString = string.Empty;
            SendDataString = string.Empty;
        }

        public Session(long id, IPAddress ip) : this()
        {
            Id = id;
            Ip = ip;
        }

        public long Id { get; set; }

        public IPAddress Ip { get; private set; }

        public bool Accepted { get; set; }

        public string StreamString { get; set; }

        public string SendDataString { get; set; }
    }
}
