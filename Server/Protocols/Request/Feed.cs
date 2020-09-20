using Server.Protocols.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebCrawler.Code;

namespace Server.Protocols.Request
{
    public class Feed
    {
        public List<Common.Rss> RssList { get; set; } = new List<Common.Rss>();

        public bool All { get; set; }
    }
}
