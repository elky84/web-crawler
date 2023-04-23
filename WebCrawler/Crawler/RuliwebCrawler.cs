using AngleSharp.Dom;
using EzAspDotNet.Util;
using MongoDB.Driver;
using Serilog;
using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using WebCrawler.Models;

namespace WebCrawler.Crawler
{
    public partial class RuliwebCrawler : CrawlerBase
    {
        public RuliwebCrawler(CrawlDataDelegate onCrawlDataDelegate, IMongoDatabase mongoDb, Source source) :
            base(onCrawlDataDelegate, mongoDb, $"https://bbs.ruliweb.com/{source.BoardId}", source)
        {
        }

        protected override string UrlComposite(int page)
        {
            return $"{UrlBase}?page={page}";
        }

        protected override void OnPageCrawl(AngleSharp.Html.Dom.IHtmlDocument document)
        {
            var cultureInfo = (CultureInfo)Thread.CurrentThread.CurrentCulture.Clone();
            var calendar = cultureInfo.Calendar;
            calendar.TwoDigitYearMax = DateTime.Now.Year + 30;
            cultureInfo.DateTimeFormat.Calendar = calendar;

            if (document.GetElementById("best_body") != null)
            {
                var rows = document.QuerySelectorAll("tr")
                    .Where(x => x.ClassName.Contains("table_body") && x.ClassName.Contains("blocktarget"))
                    .Select(x => x.TextContent.Trim())
                    .ToList();

                var tdContent = document.QuerySelectorAll("div")
                    .Where(x => x.ClassName != null && x.ClassName.Contains("article_info"))
                    .SelectMany(x => new[] { x.QuerySelectorAll("a"), x.QuerySelectorAll("span").Where(x => !string.IsNullOrEmpty(x.ClassName)) })
                    .SelectMany(x => x.Select(y => y.TextContent.Trim()))
                    .ToArray();

                var tdHref = document.QuerySelectorAll("a")
                    .Where(x => x.ClassName != null && x.ClassName.Contains("title_wrapper"))
                    .ToArray();

                if (!rows.Any() || !tdContent.Any())
                {
                    Log.Error("Parsing Failed DOM. Not has rows or tdContent {UrlComposite}", UrlComposite(1));
                    return;
                }

                var colCount = tdContent.Length / rows.Count;

                Parallel.For(0, rows.Count, n =>
                {
                    var cursor = n * colCount;
                    var category = tdContent[cursor];

                    var title = tdHref[n].TextContent;
                    title = MyRegex().Replace(title, string.Empty).Trim();

                    var author = tdContent[cursor + 1];
                    var recommend = tdContent[cursor + 2].ToIntRegex();
                    var count = tdContent[cursor + 3].ToIntRegex();

                    var dateTimeStr = tdContent[cursor + 4].Replace("날짜 ", string.Empty);
                    DateTime? date;
                    if (dateTimeStr.Contains('.'))
                    {
                        date = dateTimeStr.IndexOf('.') >= 4 ?
                            DateTime.ParseExact(dateTimeStr, "yyyy.MM.dd", cultureInfo) :
                            DateTime.ParseExact(dateTimeStr, "yy.MM.dd", cultureInfo);
                    }
                    else
                    {
                        date = DateTime.Parse(dateTimeStr);
                    }

                    var href = tdHref[n].GetAttribute("href");

                    _ = OnCrawlData(new CrawlingData
                    {
                        Type = Source.Type,
                        BoardId = Source.BoardId,
                        BoardName = Source.Name,
                        Category = category,
                        Title = title,
                        Author = author,
                        Recommend = recommend,
                        Count = count,
                        DateTime = date.GetValueOrDefault(DateTime.Now),
                        Href = href,
                        SourceId = Source.Id
                    });
                });
            }
            else
            {
                var thContent = document.QuerySelectorAll("thead tr th")
                .Select(x => x.TextContent.Trim())
                .ToList();

                var tdContent1 = document.QuerySelectorAll("tbody tr")
                    .Where(x => x.ClassName.Contains("table_body") && x.ClassName.Contains("blocktarget")).ToList();

                var tdContent = document.QuerySelectorAll("tbody tr")
                    .Where(x => x.ClassName.Contains("table_body") && x.ClassName.Contains("blocktarget"))
                    .Select(x => x.QuerySelectorAll("td"))
                    .SelectMany(x => x.Select(y => y.TextContent.Trim()))
                    .ToArray();

                var tdHref = document.QuerySelectorAll("tbody tr")
                    .Where(x => x.ClassName.Contains("table_body") && x.ClassName.Contains("blocktarget"))
                    .Select(x => x.QuerySelectorAll("td"))
                    .SelectMany(x => x.Where(y => y.ClassName == "subject" && y.QuerySelector("a") != null)
                                      .Select(y => y.QuerySelector("a").GetAttribute("href")))
                    .Where(x => x.StartsWith("http"))
                    .ToArray();

                if (!thContent.Any() || !tdContent.Any())
                {
                    Log.Error("Parsing Failed DOM. Not has thContent or tdContent {UrlComposite}", UrlComposite(1));
                    return;
                }

                Parallel.For(0, tdContent.Length / thContent.Count, n =>
                {
                    var cursor = n * thContent.Count;
                    var id = tdContent.GetValue(thContent, "ID", cursor).ToIntRegex();
                    var category = tdContent.GetValue(thContent, "구분", cursor);
                    if (string.IsNullOrEmpty(category))
                    {
                        category = tdContent.GetValue(thContent, "게시판", cursor);
                    }

                    var title = tdContent.GetValue(thContent, "제목", cursor).Substring("\n");
                    var author = tdContent.GetValue(thContent, "글쓴이", cursor);
                    var recommend = tdContent.GetValue(thContent, "추천", cursor).ToIntNullable();
                    var count = tdContent.GetValue(thContent, "조회", cursor).ToInt();

                    var dateTimeStr = tdContent.GetValue(thContent, "날짜", cursor);
                    DateTime? date;
                    if (dateTimeStr.Contains('.'))
                    {
                        date = dateTimeStr.IndexOf('.') >= 4 ?
                            DateTime.ParseExact(dateTimeStr, "yyyy.MM.dd", cultureInfo) :
                            DateTime.ParseExact(dateTimeStr, "yy.MM.dd", cultureInfo);
                    }
                    else
                    {
                        date = DateTime.Parse(dateTimeStr);
                    }

                    var href = tdHref[n].Split("?")[0];

                    _ = OnCrawlData(new CrawlingData
                    {
                        Type = Source.Type,
                        BoardId = Source.BoardId,
                        BoardName = Source.Name,
                        Category = category,
                        Title = title,
                        Author = author,
                        Recommend = recommend.GetValueOrDefault(0),
                        Count = count,
                        DateTime = date.GetValueOrDefault(DateTime.Now),
                        RowId = id,
                        Href = href,
                        SourceId = Source.Id
                    });
                });
            }
        }

        [GeneratedRegex("\\(([^)]*)\\)")]
        private static partial Regex MyRegex();
    }
}
