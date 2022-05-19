using EzMongoDb.Models;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using WebCrawler.Code;

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

        public bool Switch { get; set; } = true;
    }
}
