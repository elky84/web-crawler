using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace WebUtil.Models
{
    public class MongoDbHeader
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }


        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime Created { get; set; } = DateTime.Now;
    }
}
