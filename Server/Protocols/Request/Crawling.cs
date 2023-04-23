using System.Collections.Generic;

namespace Server.Protocols.Request
{
    public class Crawling : EzAspDotNet.Protocols.RequestHeader
    {
        public List<Common.Source> Sources { get; set; }

        public bool All { get; init; }
    }
}
