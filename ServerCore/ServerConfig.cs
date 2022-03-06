namespace ServerCore
{
    internal static class ServerConfig
    {
        static public string CloseConnectionCommand { get => "bb"; }
        static public char CommandArgsSplitter { get => '|'; }
        static public int DataReadPacketSize { get => 64; }
        public static double ClientSecondsLifetime { get => 10; }
    }
}
