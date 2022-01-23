using System.Collections.Generic;


namespace Server.Protocols.Response
{
    public class CrawlingList : Pagable
    {
        public List<Common.CrawlingData> Datas = new List<Common.CrawlingData>();
    }
}
