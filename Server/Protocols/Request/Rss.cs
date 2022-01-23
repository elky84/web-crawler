
namespace Server.Protocols.Request
{
    public class Rss : EzAspDotNet.Protocols.RequestHeader
    {
        public Common.Rss Data { get; set; }
    }
}
