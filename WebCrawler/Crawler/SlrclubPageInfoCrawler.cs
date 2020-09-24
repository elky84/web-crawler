using MongoDB.Driver;
using System.Linq;
using System.Threading.Tasks;
using WebCrawler.Models;
using WebUtil.Util;

namespace WebCrawler.Crawler
{
    public class SlrclubPageInfoCrawler : CrawlerBase
    {
        public int? LatestPage { get; set; }

        public SlrclubPageInfoCrawler(CrawlDataDelegate onCrawlDataDelegate, IMongoDatabase mongoDb, Source source) :
            base(onCrawlDataDelegate, mongoDb, $"http://www.slrclub.com/bbs/zboard.php", source)
        {
        }

        protected override string UrlComposite(int page)
        {
            return $"{UrlBase}?id={Source.BoardId}&page={page}";
        }

        public override async Task RunAsync()
        {
            if (CrawlerInstance == null)
            {
                Create();
            }

            // 전체 페이지를 알아오기 위한 SlrClub용 우회이므로, 그냥 1페이지를 호출한다.
            await ExecuteAsync(1);
        }

        protected override void OnPageCrawl(AngleSharp.Html.Dom.IHtmlDocument document)
        {
            var tdContent = document.QuerySelectorAll("tbody tr td table tbody tr td span").Select(x => x.TextContent.Trim()).ToArray();
            var latest = tdContent.LastOrDefault();
            LatestPage = string.IsNullOrEmpty(latest) ? (int?)null : latest.ToInt();
        }
    }
}
