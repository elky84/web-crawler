using System.Collections.Generic;

namespace Server.Protocols.Response
{
    public class FeedList : Pagable
    {
        public List<Common.FeedData> Datas = new List<Common.FeedData>();
    }
}
