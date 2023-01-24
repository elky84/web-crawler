using EzAspDotNet.Util;
using MongoDB.Driver;
using Serilog;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WebCrawler.Models;

namespace WebCrawler.Crawler
{
    public class SlrclubCrawler : CrawlerBase<SlrclubCrawler>
    {
        protected int? LatestPage { get; set; }

        public SlrclubCrawler(CrawlDataDelegate onCrawlDataDelegate, IMongoDatabase mongoDb, Source source) :
            base(onCrawlDataDelegate, mongoDb, $"http://www.slrclub.com/bbs/zboard.php", source)
        {
        }

        protected override string UrlComposite(int page)
        {
            return $"{UrlBase}?id={Source.BoardId}&page={LatestPage - page - 1}";
        }

        public override async Task RunAsync()
        {
            Create();

            var pageInfoCrawler = new SlrclubPageInfoCrawler(null, null, Source);
            await pageInfoCrawler.RunAsync().WaitAsync(TimeSpan.FromSeconds(10));
            LatestPage = pageInfoCrawler.LatestPage;

            if (!LatestPage.HasValue)
            {
                Log.Logger.Error($"Not Found PageInfo Data {UrlBase}");
                return;
            }


            for (var page = Source.PageMin; page <= Source.PageMax; ++page)
            {
                await ExecuteAsync(page);
                Thread.Sleep(Source.Interval);
            }
        }

        protected override void OnPageCrawl(AngleSharp.Html.Dom.IHtmlDocument document)
        {
            var thContent = document.QuerySelectorAll("thead tr th")
                .Select(x => x.TextContent.Trim()).ToArray();

            var tdContent = document.QuerySelectorAll("tbody tr td")
                .Select(x =>
                {
                    return x.QuerySelector("a") != null ? x.QuerySelector("a").TextContent.Trim() : x.TextContent.Trim();
                })
                .ToArray();

            var tdHref = document.QuerySelectorAll("tbody tr td")
                .Where(x => !string.IsNullOrEmpty(x.ClassName) && x.ClassName.Contains("sbj"))
                .Select(x => x.QuerySelector("a").GetAttribute("href"))
                .ToArray();

            if (!thContent.Any() || !tdContent.Any())
            {
                Log.Error($"Parsing Failed DOM. Not has thContent or tdContent {UrlComposite(1)}");
                return;
            }

            Parallel.For(0, tdContent.Length / thContent.Length, n =>
            {
                var cursor = n * thContent.Length;
                var id = tdContent[cursor + 0].ToInt();
                var title = tdContent[cursor + 1];
                var author = tdContent[cursor + 2];
                var date = DateTime.Parse(tdContent[cursor + 3]);
                var recommend = tdContent[cursor + 4].ToInt();
                var count = tdContent[cursor + 5].ToInt();

                var href = UrlCompositeHref(tdHref[n]);

                _ = OnCrawlData(new CrawlingData
                {
                    Type = Source.Type,
                    BoardId = Source.BoardId,
                    BoardName = Source.Name,
                    RowId = id,
                    Title = title,
                    Author = author,
                    Recommend = recommend,
                    Count = count,
                    DateTime = date,
                    Href = href,
                    SourceId = Source.Id
                });
            });
        }
    }
}
