﻿using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using WebCrawler.Code;

namespace Server.Protocols.Common
{
    public class Source : Header
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public CrawlingType Type { get; set; }

        public string BoardId { get; set; }

        public string Name { get; set; }

        public int PageMin { get; set; } = 1;

        public int PageMax { get; set; } = 1;

        public int Interval { get; set; } = 1000;

        public bool Switch { get; set; } = true;
    }
}
