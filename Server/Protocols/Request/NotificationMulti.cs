using System.Collections.Generic;

namespace Server.Protocols.Request
{
    public class NotificationMulti : EzAspDotNet.Protocols.RequestHeader
    {
        public List<Common.NotificationCreate> Datas { get; set; }
    }
}
