using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebCrawler.Code;

namespace Server.Protocols.Common
{
    public class Rss : Header
    {
        public string Url { get; set; }

        public string Name { get; set; }

        public int Day { get; set; } = 7;

        public DateTime? ErrorTime { get; set; }

        public string Error { get; set; }
    }
}
