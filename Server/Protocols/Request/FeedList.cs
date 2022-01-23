using WebCrawler.Code;

namespace Server.Protocols.Request
{
    public class FeedList : EzAspDotNet.Protocols.Page.Pageable
    {
        public string Keyword { get; set; }
    }
}
