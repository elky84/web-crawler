using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Server.Protocols.Notification.Request
{
    public class SlackWebHook
    {
#pragma warning disable IDE1006 // Naming Styles
        public string text { get; set; }

        public string username { get; set; }

        public string icon_url { get; set; }

        public string channel { get; set; }
#pragma warning restore IDE1006 // Naming Styles
    }
}
