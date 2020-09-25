using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebCrawler.Code;
using WebUtil.Models;

namespace WebCrawler.Models
{
    public class Source : MongoDbHeader
    {
        public string BoardId { get; set; }

        [BsonRepresentation(BsonType.String)]
        public CrawlingType Type { get; set; }

        public string Name { get; set; }

        public int PageMin { get; set; } = 1;

        public int PageMax { get; set; } = 5;

        public int Interval { get; set; } = 1;
    }
}
