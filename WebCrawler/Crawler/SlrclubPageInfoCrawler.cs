using EzAspDotNet.Util;
using MongoDB.Driver;
using System.Linq;
using System.Threading.Tasks;
using WebCrawler.Models;

namespace WebCrawler.Crawler
{
    public class SlrclubPageInfoCrawler(CrawlDataDelegate onCrawlDataDelegate, IMongoDatabase mongoDb, Source source)
        : CrawlerBase(onCrawlDataDelegate, mongoDb, $"http://www.slrclub.com/bbs/zboard.php", source)
    {
        public static int? LatestPage { get; set; }

        protected override string UrlComposite(int page)
        {
            return $"{UrlBase}?id={Source.BoardId}&page={page}";
        }

        public override async Task RunAsync()
        {
            var crawlerInstance = Create();

            // 전체 페이지를 알아오기 위한 SlrClub용 우회이므로, 그냥 1페이지를 호출한다.
            await ExecuteAsync(crawlerInstance, 1);
        }

        protected override void OnPageCrawl(AngleSharp.Html.Dom.IHtmlDocument document)
        {
            var tdContent = document.QuerySelectorAll("tbody tr td table tbody tr td span").Select(x => x.TextContent.Trim()).ToArray();
            if (tdContent.Length == 0)
            {
                return;
            }

            var latest = tdContent.LastOrDefault();
            LatestPage = string.IsNullOrEmpty(latest) ? (int?)null : latest.ToInt();
        }
    }
}
