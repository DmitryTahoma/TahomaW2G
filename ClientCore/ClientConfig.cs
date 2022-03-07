namespace ClientCore
{
    internal static class ClientConfig
    {
        static public string CloseConnectionCommand { get => "bb"; }
        //static public string StartMessagePart { get => "<\b\rrq\r\b>"; }
        //static public string EndMessagePart { get => "</\b\rrq\r\b>"; }
        static public string StartMessagePart { get => "<rq>"; }
        static public string EndMessagePart { get => "</rq>"; }
        static public string CommandArgsSplitter { get => "|"; }
        static public int WaitDataDelay { get => 10; }
        static public int WaitServerConnectionDelay { get => 25; }
        static public int DataReadPacketSize { get => 64; }
    }
}
