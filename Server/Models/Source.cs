using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebCrawler.Code;

namespace Server.Models
{
    public class Source
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        public string BoardId { get; set; }

        public CrawlingType Type { get; set; }

        public Protocols.Common.Source ToProtocol()
        {
            return new Protocols.Common.Source
            {
                Id = Id,
                BoardId = BoardId,
                Type = Type
            };
        }
    }
}
