using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebUtil.Models;

namespace FeedCrawler.Models
{
    public class FeedData : MongoDbHeader
    {
        public string FeedTitle { get; set; }

        public string Description { get; set; }

        public string Href { get; set; }

        public DateTime DateTime { get; set; }

        public string Url { get; set; }

        public string ItemTitle { get; set; }

        public string ItemAuthor { get; set; }

        public string ItemContent { get; set; }
    }
}
