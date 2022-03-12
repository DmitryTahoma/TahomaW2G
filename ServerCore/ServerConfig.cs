namespace ServerCore
{
    internal static class ServerConfig
    {
        static public string CloseConnectionCommand { get => "bb"; }
        //static public string StartMessagePart { get => "<\b\rrq\r\b>"; }
        //static public string EndMessagePart { get => "</\b\rrq\r\b>"; }
        static public string StartMessagePart { get => "<rq>"; }
        static public string EndMessagePart { get => "</rq>"; }
        static public char CommandArgsSplitter { get => '|'; }
        static public int DataReadPacketSize { get => 64; }
        public static double ClientSecondsLifetime { get => 10; }
        public static double WaitAcceptClientSeconds { get => 3; }
    }
}
