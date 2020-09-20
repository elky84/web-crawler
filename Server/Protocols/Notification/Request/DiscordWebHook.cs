using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Server.Protocols.Notification.Request
{
    public class DiscordWebHook
    {
#pragma warning disable IDE1006 // Naming Styles
        public string content { get; set; }

        public string username { get; set; }

        public string avatar_url { get; set; }
#pragma warning restore IDE1006 // Naming Styles
    }
}
