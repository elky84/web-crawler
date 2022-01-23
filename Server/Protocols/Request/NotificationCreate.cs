
namespace Server.Protocols.Request
{
    public class NotificationCreate : EzAspDotNet.Protocols.RequestHeader
    {
        public Common.NotificationCreate Data { get; set; }
    }
}
