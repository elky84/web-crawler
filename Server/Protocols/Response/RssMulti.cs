using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Server.Protocols.Response
{
    public class RssMulti : Header
    {
        public List<Common.Rss> Datas { get; set; }
    }
}
