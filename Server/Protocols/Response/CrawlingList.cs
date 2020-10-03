using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Server.Protocols.Response
{
    public class CrawlingList : Pagable
    {
        public List<Common.CrawlingData> Datas = new List<Common.CrawlingData>();
    }
}
