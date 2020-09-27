using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using System;

namespace WebUtil.Models
{
    public class MongoDbHeader
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        [JsonProperty("_Id")]
        public string Id { get; set; }


        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime Created { get; set; } = DateTime.Now;


        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime Updated { get; set; }
    }
}
