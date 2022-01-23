﻿using System;

namespace Server.Protocols.Common
{
    public class FeedData : Header
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
