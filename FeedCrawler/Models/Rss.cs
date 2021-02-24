using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using WebUtil.Models;

namespace FeedCrawler.Models
{
    public class Rss : MongoDbHeader
    {
        public string Url { get; set; }

        public string Name { get; set; }

        public DateTime? ErrorTime { get; set; }

        public string Error { get; set; }
    }
}
