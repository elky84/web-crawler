using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using WebCrawler.Code;

namespace Server.Protocols.Common
{
    public class CrawlingData
    {
        public string Id { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public CrawlingType Type { get; set; }

        public string BoardId { get; set; }

        public string BoardName { get; set; }

        public string Author { get; set; }

        public string SourceId { get; set; }

        public string Href { get; set; }

        public string Category { get; set; }

        public int Count { get; set; }

        public int Recommend { get; set; }

        public string Title { get; set; }

        public DateTime DateTime { get; set; }

        public long? RowId { get; set; }
    }
}
