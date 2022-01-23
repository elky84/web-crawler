using System.Collections.Generic;

namespace Server.Protocols.Request
{
    public class RssMulti : EzAspDotNet.Protocols.RequestHeader
    {
        public List<Common.Rss> Datas { get; set; }
    }
}
