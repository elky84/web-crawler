using MongoDB.Driver;
using Serilog;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WebCrawler.Models;
using MongoDbWebUtil.Util;

namespace WebCrawler.Crawler
{
    public class HumorUnivCrawler : CrawlerBase
    {
        protected int? LatestPage { get; set; }

        public HumorUnivCrawler(CrawlDataDelegate onCrawlDataDelegate, IMongoDatabase mongoDb, Source source) :
            base(onCrawlDataDelegate, mongoDb, $"http://web.humoruniv.com/board/humor/list.html?table=", source)
        {
        }

        protected override string UrlComposite(int page)
        {
            return $"{UrlBase}{Source.BoardId}&pg={page - 1}";
        }

        protected override string UrlCompositeHref(string href)
        {
            return UrlBase.CutAndComposite("/", 0, 5, "/" + href);
        }

        protected override void OnPageCrawl(AngleSharp.Html.Dom.IHtmlDocument document)
        {
            var tdContent = document.QuerySelectorAll("tbody tr td")
                .Where(x => !string.IsNullOrEmpty(x.ClassName) && x.ClassName != "g6")
                .Select(x =>
                {
                    return x.QuerySelector("a") != null ? x.QuerySelector("a").TextContent.Trim() : x.TextContent.Trim();
                })
                .ToArray();

            var tdHref = document.QuerySelectorAll("tbody tr td a")
                .Where(x => !string.IsNullOrEmpty(x.ClassName))
                .Select(x => x.GetAttribute("href"))
                .ToArray();


            if (!tdContent.Any())
            {
                return;
            }

            const int thLength = 7;
            var thContent = tdContent.Take(thLength);
            tdContent = tdContent.Skip(thLength).ToArray();

            Parallel.For(0, tdContent.Length / thLength, n =>
            {
                var cursor = n * thLength;

                var originTitle = tdContent[cursor + 1];

                var title = originTitle.Substring("\n");
                var author = tdContent[cursor + 2];
                var date = DateTime.Parse(tdContent[cursor + 3]);
                var count = tdContent[cursor + 4].ToInt();
                var recommend = tdContent[cursor + 5].ToInt();
                var notRecommend = tdContent[cursor + 6].ToInt();

                var href = UrlCompositeHref(tdHref[n]);

                _ = OnCrawlData(new CrawlingData
                {
                    Type = Source.Type,
                    BoardId = Source.BoardId,
                    BoardName = Source.Name,
                    Title = title,
                    Author = author,
                    Recommend = recommend - notRecommend,
                    Count = count,
                    DateTime = date,
                    Href = href,
                    SourceId = Source.Id
                });
            });
        }
    }
}
