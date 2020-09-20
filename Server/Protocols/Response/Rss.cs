using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Server.Protocols.Response
{
    public class Rss : Header
    {
        public Common.Rss RssData { get; set; }
    }
}
