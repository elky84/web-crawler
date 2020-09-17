using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Server.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

        public string BoardName { get; set; }

        public int CountBaseLine { get; set; }

        public int RecommendBaseLine { get; set; }
    }
}
