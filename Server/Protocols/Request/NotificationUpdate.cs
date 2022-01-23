
namespace Server.Protocols.Request
{
    public class NotificationUpdate : EzAspDotNet.Protocols.RequestHeader
    {
        public Common.Notification Data { get; set; }
    }
}
