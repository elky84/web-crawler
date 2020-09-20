using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Server.Code;
using System;
using WebCrawler.Code;

namespace Server.Models
{
    public class Notification
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonRepresentation(BsonType.String)]
        public NotificationType Type { get; set; }

        public string Name { get; set; }

        public string HookUrl { get; set; }

        public string Channel { get; set; }

        public string IconUrl { get; set; }

        [BsonRepresentation(BsonType.String)]
        public CrawlingType CrawlingType { get; set; }

        public string SourceId { get; set; }
    }
}
