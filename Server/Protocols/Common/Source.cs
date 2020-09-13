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
    }
}
