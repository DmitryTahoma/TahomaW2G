using System;
using System.Threading.Tasks;

namespace ClientCore
{
    public class ClientAsync
    {
        private readonly Client client;

        private bool isWaitResponse;
        private string response;

        public ClientAsync(string ip, int port)
        {
            client = new Client(ip, port);
            client.OnGettingMessage += OnGettingMessage;

            isWaitResponse = false;
            response = string.Empty;
        }

        public async void Connect()
        {
            await Task.Run(() => {
                client.Connect();
                client.StartListenningResponse();
            });
        }

        public async Task<string> Send(string message)
        {
            return await Task.Run(() => 
            {
                isWaitResponse = true;

                while (isWaitResponse)
                {
                    DateTime startTime = DateTime.Now;
                    client.Push(message);

                    while (isWaitResponse && (DateTime.Now - startTime).TotalSeconds <= 3)
                        Task.Delay(25);
                }

                string res = response;
                response = string.Empty;
                return res;
            });
        }

        private void OnGettingMessage(string response)
        {
            if(isWaitResponse)
            {
                this.response = response;
                isWaitResponse = false;
            }
        }
    }
}
