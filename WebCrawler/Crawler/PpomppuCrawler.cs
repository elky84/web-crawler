using EzAspDotNet.Util;
using MongoDB.Driver;
using Serilog;
using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AngleSharp.Dom;
using System.Text;
using WebCrawler.Models;

namespace WebCrawler.Crawler
{
    public class PpomppuCrawler(CrawlDataDelegate onCrawlDataDelegate, IMongoDatabase mongoDb, Source source)
        : CrawlerBase(onCrawlDataDelegate, mongoDb, $"https://www.ppomppu.co.kr/zboard/zboard.php", source)
    {
        protected override string UrlComposite(int page)
        {
            return $"{UrlBase}?id={Source.BoardId}&page={page}";
        }

        protected override string UrlCompositeHref(string href)
        {
            return UrlBase.CutAndComposite("/", 0, 4, href);
        }

        protected override void OnPageCrawl(IDocument document)
        {
            var cultureInfo = (CultureInfo)Thread.CurrentThread.CurrentCulture.Clone();
            var calendar = cultureInfo.Calendar;
            calendar.TwoDigitYearMax = DateTime.Now.Year + 30;
            cultureInfo.DateTimeFormat.Calendar = calendar;
            
            var thContent = document.QuerySelectorAll("tbody tr")
                .Where(x => x.Id == "headNotice" || x.ClassName == "title_bg")
                .Select(x => x.QuerySelectorAll("span, font"))
                .SelectMany(x => x.Select(y => y.TextContent.Trim()))
                .ToArray();
            
            var tdContent = document.QuerySelectorAll("tbody tr")
                .Where(x => !string.IsNullOrEmpty(x.ClassName) && x.ClassName.StartsWith("baseList"))
                .Select(x => x.QuerySelectorAll("td").Where(x => !string.IsNullOrEmpty(x.ClassName) && x.ClassName.StartsWith("baseList-space")))
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
                .Where(x => !string.IsNullOrEmpty(x.ClassName) && x.ClassName.StartsWith("baseList"))
                .Select(x => x.QuerySelectorAll("td a"))
                .SelectMany(x => x.Select(y => y.GetAttribute("href")))
                .Where(x => x != "#")
                .ToArray();

            if (thContent.Length == 0 || tdContent.Length == 0)
            {
                Log.Error("Parsing Failed DOM. Not has thContent or tdContent {UrlComposite}", UrlComposite(1));
                return;
            }

            for(var n = 0; n < tdContent.Length / thContent.Length; ++n)
            {
                var cursor = n * thContent.Length;

                if (!int.TryParse(tdContent[cursor + 0], out var id))
                {
                    return;
                }
                
                var title = tdContent[cursor + 1].Split(["\r\n", "\n", "\r"], StringSplitOptions.None)[0].Trim();
                var author = tdContent[cursor + 2];
                var dateTimeStr = tdContent[cursor + 3];
                var date = dateTimeStr.Contains('/') ? DateTime.ParseExact(dateTimeStr, "yy/MM/dd", cultureInfo) : DateTime.Parse(dateTimeStr);

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
            }
        }
    }
}
