using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using WebUtil.Models;

namespace FeedCrawler.Models
{
    public class Rss : MongoDbHeader
    {
        public string Url { get; set; }

        public string Name { get; set; }

        public int Day { get; set; } = 7;
    }
}
