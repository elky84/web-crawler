using EzAspDotNet.Notification.Types;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;

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

        public string Keyword { get; set; }

        public string Prefix { get; set; }

        public string Postfix { get; set; }

        public string FilterKeyword { get; set; }

        public string CrawlingType { get; set; }

        [JsonProperty(ItemConverterType = typeof(StringEnumConverter))]
        public List<DayOfWeek> FilterDayOfWeeks { get; set; } = new();

        public string FilterStartTime { get; set; }

        public string FilterEndTime { get; set; }
    }
}
