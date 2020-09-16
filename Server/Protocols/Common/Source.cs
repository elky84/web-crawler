using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebCrawler.Code;

namespace Server.Protocols.Common
{
    public class Source
    {
        public string Id { get; set; }

        public CrawlingType Type { get; set; }

        public string BoardId { get; set; }

        public string Name { get; set; }

        public int PageMin { get; set; } = 1;

        public int PageMax { get; set; } = 10;

        public int Interval { get; set; } = 1000;
    }
}
