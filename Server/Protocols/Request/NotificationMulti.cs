using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebCrawler.Code;

namespace Server.Protocols.Request
{
    public class NotificationMulti
    {
        public List<Common.NotificationCreate> Datas { get; set; }
    }
}
