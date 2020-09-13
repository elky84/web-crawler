using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebCrawler.Code;

namespace WebCrawler.Models
{
    public class Source
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        public string BoardId { get; set; }

        [BsonRepresentation(BsonType.String)]
        public CrawlingType Type { get; set; }

        public string Name { get; set; }

    }
}
