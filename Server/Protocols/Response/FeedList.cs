using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Server.Protocols.Response
{
    public class FeedList : Pagable
    {
        public List<Common.FeedData> FeedDatas = new List<Common.FeedData>();
    }
}
