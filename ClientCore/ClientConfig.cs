namespace ClientCore
{
    internal static class ClientConfig
    {
        static public string CloseConnectionCommand { get => "bb"; }
        static public string CommandArgsSplitter { get => "|"; }
        static public int WaitDataDelay { get => 10; }
        static public int DataReadPacketSize { get => 64; }
    }
}
