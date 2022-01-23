using WebCrawler.Code;

namespace Server.Protocols.Request
{
    public class CrawlingList : EzAspDotNet.Protocols.Page.Pageable
    {
        public string Keyword { get; set; }

        public CrawlingType? Type { get; set; }
    }
}
