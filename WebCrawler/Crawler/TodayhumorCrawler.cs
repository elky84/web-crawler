using EzAspDotNet.Util;
using MongoDB.Driver;
using Serilog;
using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WebCrawler.Models;

namespace WebCrawler.Crawler
{
    public class TodayhumorCrawler : CrawlerBase
    {
        public TodayhumorCrawler(CrawlDataDelegate onCrawlDataDelegate, IMongoDatabase mongoDb, Source source) :
            base(onCrawlDataDelegate, mongoDb, $"http://www.todayhumor.co.kr/board/list.php", source)
        {
        }

        protected override string UrlComposite(int page)
        {
            return $"{UrlBase}?table={Source.BoardId}&page={page}";
        }

        protected override void OnPageCrawl(AngleSharp.Html.Dom.IHtmlDocument document)
        {
            var thContent = document.QuerySelectorAll("thead tr th")
                .Select(x => x.TextContent.Trim())
                .ToArray();

            var tdContent = document.QuerySelectorAll("tbody tr")
                .Where(x => x.ClassName != null && x.ClassName.StartsWith("view list_tr_"))
                .Select(x => x.QuerySelectorAll("td"))
                .SelectMany(x => x.Select(y =>
                {
                    return y.QuerySelector("a") != null ? y.QuerySelector("a").TextContent.Trim() : y.TextContent.Trim();
                }))
                .ToArray();

            var tdHref = document.QuerySelectorAll("tbody tr td")
                .Where(x => x.ClassName == ("subject"))
                .Select(x => x.QuerySelector("a").GetAttribute("href"))
                .ToArray();

            if (!thContent.Any() || !tdContent.Any())
            {
                Log.Error($"Parsing Failed DOM. Not has thContent or tdContent {UrlComposite(1)}");
                return;
            }

            var cultureInfo = (CultureInfo)Thread.CurrentThread.CurrentCulture.Clone();
            var calendar = cultureInfo.Calendar;
            calendar.TwoDigitYearMax = DateTime.Now.Year + 30;
            cultureInfo.DateTimeFormat.Calendar = calendar;

            Parallel.For(0, tdContent.Length / thContent.Length, n =>
            {
                var cursor = n * thContent.Length;
                var id = tdContent[cursor + 0].ToIntRegex();
                var title = tdContent[cursor + 2];
                var author = tdContent[cursor + 3];
                var date = DateTime.ParseExact(tdContent[cursor + 4], "yy/MM/dd HH:mm", cultureInfo);
                var count = tdContent[cursor + 5].ToInt();
                var recommend = tdContent[cursor + 6].Split("/")[0].ToIntRegex();

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
