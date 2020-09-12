using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebCrawler.Code;

namespace Server.Protocols.Request
{
    public class Source
    {
        public CrawlingType Type { get; set; }

        public string BoardId { get; set; }
    }
}
