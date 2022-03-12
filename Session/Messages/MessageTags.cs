namespace SessionLib.Messages
{
    public static class MessageTags
    {
        //static public string StartMessagePart { get => "<\b\rrq\r\b>"; }
        //static public string EndMessagePart { get => "</\b\rrq\r\b>"; }
        static public string StartMessagePart { get => "<rq>"; }
        static public string EndMessagePart { get => "</rq>"; }
    }
}
