using EzAspDotNet.Notification.Types;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using WebCrawler.Code;

namespace Server.Protocols.Common
{
    public class NotificationCreate
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public NotificationType Type { get; set; }

        public string Name { get; set; }

        public string HookUrl { get; set; }

        public string Channel { get; set; }

        public string IconUrl { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public CrawlingType CrawlingType { get; set; }

        public string BoardName { get; set; }

        public string Keyword { get; set; }

        public string Prefix { get; set; }

        public string Postfix { get; set; }

        public string FilterKeyword { get; set; }

        [JsonProperty(ItemConverterType = typeof(StringEnumConverter))]
        public List<DayOfWeek> FilterDayOfWeeks { get; set; } = new List<DayOfWeek>();

        public string FilterStartTime { get; set; }

        public string FilterEndTime { get; set; }
    }
}
