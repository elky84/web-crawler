﻿using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Server.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebCrawler.Code;

namespace Server.Protocols.Common
{
    public class Notification : Header
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public NotificationType Type { get; set; }

        public string Name { get; set; }

        public string HookUrl { get; set; }

        public string Channel { get; set; }

        public string IconUrl { get; set; }

        public string SourceId { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public CrawlingType CrawlingType { get; set; }

        public string Keyword { get; set; }

        [JsonProperty(ItemConverterType = typeof(StringEnumConverter))]
        public List<DayOfWeek> FilterDayOfWeeks { get; set; } = new List<DayOfWeek>();

        public string FilterStartTime { get; set; }

        public string FilterEndTime { get; set; }
    }
}
