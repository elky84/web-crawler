using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebCrawler.Code;

namespace WebCrawler.Models
{
    public class CrawlingData
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        public CrawlingType Type { get; set; }

        public string BoardId { get; set; }

        public string Author { get; set; }

        public string Href { get; set; }

        public string Category { get; set; }

        public int Count { get; set; }

        public int Recommend { get; set; }

        public string Title { get; set; }

        public DateTime DateTime { get; set; }

        public long? RowId { get; set; }
    }
}
