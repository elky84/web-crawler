using System.Collections.Generic;

namespace Server.Protocols.Request
{
    public class Feed : EzAspDotNet.Protocols.RequestHeader
    {
        public List<Common.Rss> RssList { get; set; } = new List<Common.Rss>();

        public bool All { get; set; }
    }
}
