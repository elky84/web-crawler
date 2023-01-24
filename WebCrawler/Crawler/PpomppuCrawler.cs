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
    public class PpomppuCrawler : CrawlerBase<PpomppuCrawler>
    {
        public PpomppuCrawler(CrawlDataDelegate onCrawlDataDelegate, IMongoDatabase mongoDb, Source source) :
            base(onCrawlDataDelegate, mongoDb, $"https://www.ppomppu.co.kr/zboard/zboard.php", source)
        {
        }

        protected override string UrlComposite(int page)
        {
            return $"{UrlBase}?id={Source.BoardId}&page={page}";
        }

        protected override string UrlCompositeHref(string href)
        {
            return UrlBase.CutAndComposite("/", 0, 4, href);
        }

        protected override void OnPageCrawl(AngleSharp.Html.Dom.IHtmlDocument document)
        {
            var cultureInfo = (CultureInfo)Thread.CurrentThread.CurrentCulture.Clone();
            var calendar = cultureInfo.Calendar;
            calendar.TwoDigitYearMax = DateTime.Now.Year + 30;
            cultureInfo.DateTimeFormat.Calendar = calendar;

            var thContent = document.QuerySelectorAll("tbody tr")
                .Where(x => x.ClassName == "title_bg")
                .Select(x => x.QuerySelectorAll("td").Where(x => x.ClassName == "list_tspace"))
                .SelectMany(x => x.Select(y => y.TextContent.Trim()))
                .ToArray();

            var tdContent = document.QuerySelectorAll("tbody tr")
                .Where(x => !string.IsNullOrEmpty(x.ClassName) && !x.ClassName.Contains("list_notice"))
                .Select(x => x.QuerySelectorAll("td").Where(x => !string.IsNullOrEmpty(x.ClassName) && x.ClassName.Contains("list_vspace")))
                .SelectMany(x => x.Select(y =>
                {
                    var text = y.TextContent.Trim();
                    if (string.IsNullOrEmpty(text))
                    {
                        text = y.QuerySelector("img")?.GetAttribute("alt");
                    }
                    if (y.QuerySelector("font") != null)
                    {
                        text = y.QuerySelector("font").TextContent.Trim();
                    }

                    return text;
                }))
                .ToArray();

            var tdHref = document.QuerySelectorAll("tbody tr")
                .Where(x => !string.IsNullOrEmpty(x.ClassName) && !x.ClassName.Contains("list_notice"))
                .Select(x => x.QuerySelectorAll("td a"))
                .SelectMany(x => x.Where(y => y.QuerySelector("font") != null)
                    .Select(y => y.GetAttribute("href")))
                .Where(x => x != "#")
                .ToArray();

            if (!thContent.Any() || !tdContent.Any())
            {
                Log.Error($"Parsing Failed DOM. Not has thContent or tdContent {UrlComposite(1)}");
                return;
            }

            Parallel.For(0, tdContent.Length / thContent.Length, n =>
            {
                var cursor = n * thContent.Length;

                if (!int.TryParse(tdContent[cursor + 0], out var id))
                {
                    return;
                }

                var author = tdContent[cursor + 1];
                var title = tdContent[cursor + 2];
                var dateTimeStr = tdContent[cursor + 3];
                DateTime date;
                if (dateTimeStr.Contains('/'))
                {
                    date = DateTime.ParseExact(dateTimeStr, "yy/MM/dd", cultureInfo);
                }
                else
                {
                    date = DateTime.Parse(dateTimeStr);
                }


                var str = tdContent[cursor + 4];
                var recommend = string.IsNullOrEmpty(str) ? 0 : str.Split(" - ")[0].ToInt();

                var count = tdContent[cursor + 5].ToInt();

                var href = UrlCompositeHref("/" + tdHref[n]);

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
