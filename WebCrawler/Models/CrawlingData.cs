using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebCrawler.Code;
using EzAspDotNet.Models;

namespace WebCrawler.Models
{
    public class CrawlingData : MongoDbHeader
    {
        [BsonRepresentation(BsonType.String)]
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
