using System.Net;
using System.Threading.Tasks;

namespace ClientCore
{
    public class ClientAsync : Client
    {
        public ClientAsync(IPAddress ip, int port, long sessionId) : base(ip, port, sessionId) { }
        public ClientAsync(IPAddress ip, int port) : base(ip, port) { }
        public ClientAsync(string ip, int port) : base(ip, port) { }
        public ClientAsync(string ip, int port, long sessionId) : base(ip, port, sessionId) { }

        public async void ConnectAsync()
        {
            await Task.Run(() => {
                Connect();
            });
        }

        public async void StartListenningResponseAsync()
        {
            await Task.Run(() => {
                StartListenningResponse();
            });
        }

        public async void PushAsync(string message)
        {
            await Task.Run(() => {
                Push(message);
            });
        }
    }
}
