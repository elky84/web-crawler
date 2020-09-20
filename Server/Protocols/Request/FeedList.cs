using WebCrawler.Code;

namespace Server.Protocols.Request
{
    public class FeedList : Pagable
    {
        public string Keyword { get; set; }
    }
}
