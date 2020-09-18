using WebCrawler.Code;

namespace Server.Protocols.Request
{
    public class CrawlingList : Pagable
    {
        public string Keyword { get; set; }

        public CrawlingType? Type { get; set; }
    }
}
