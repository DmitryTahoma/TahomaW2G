namespace ClientCore
{
    internal static class ClientConfig
    {
        static public string CloseConnectionCommand { get => "bb"; }
        static public int WaitServerConnectionDelay { get => 25; }
        static public int DataReadPacketSize { get => 64; }
    }
}
