using System.Collections.Generic;

namespace Server.Protocols.Response
{
    public class NotificationMulti : EzAspDotNet.Protocols.ResponseHeader
    {
        public List<Common.Notification> Datas { get; set; }
    }
}
