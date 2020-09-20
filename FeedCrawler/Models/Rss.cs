using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace FeedCrawler.Models
{
    public class Rss
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        public string Url { get; set; }

        public string Name { get; set; }

        public int Day { get; set; } = 7;
    }
}
