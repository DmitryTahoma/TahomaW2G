namespace SessionLib.Messages
{
    public static class MessageFormatter
    {
        public static bool HaveTaggedMessage(string msg, string tagStart, string tagEnd)
        {
            if (string.IsNullOrEmpty(msg))
                return false;

            return msg.IndexOf(tagStart) >= 0 && msg.IndexOf(tagEnd) >= 0;
        }

        public static string SubstringFromTags(string msg, string tagStart, string tagEnd)
        {
            if (!HaveTaggedMessage(msg, tagStart, tagEnd)) 
                return string.Empty;

            int indexStart = msg.IndexOf(tagStart);
            int indexEnd = msg.IndexOf(tagEnd);

            if(indexStart >= 0 && indexEnd >= 0)
            {
                return msg.Substring(indexStart + tagStart.Length, indexEnd - tagEnd.Length + 1);
            }

            return string.Empty;
        }

        public static string CutMessageFromTags(string msg, string tagStart, string tagEnd)
        {
            if (!HaveTaggedMessage(msg, tagStart, tagEnd))
                return null;

            int indexStart = msg.IndexOf(tagEnd) + tagEnd.Length;
            int length = msg.Length - indexStart;

            return msg.Substring(indexStart, length);
        }
    }
}
