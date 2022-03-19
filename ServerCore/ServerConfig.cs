namespace ServerCore
{
    internal static class ServerConfig
    {
        static public string CloseConnectionCommand { get => "bb"; }
        static public int DataReadPacketSize { get => 64; }
        public static double ClientSecondsLifetime { get => 180; }
        public static double WaitAcceptClientSeconds { get => 3; }
    }
}
