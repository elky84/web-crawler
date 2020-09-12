using Server.Protocols.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebCrawler.Code;

namespace Server.Protocols.Request
{
    public class Crawling
    {
        public int Page { get; set; }

        public List<Common.Source> Sources { get; set; }

        public bool All { get; set; }
    }
}
