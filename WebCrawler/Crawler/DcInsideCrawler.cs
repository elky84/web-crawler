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
    public partial class DcInsideCrawler(CrawlDataDelegate onCrawlDataDelegate, IMongoDatabase mongoDb, Source source)
        : CrawlerBase(onCrawlDataDelegate, mongoDb, $"https://gall.dcinside.com/{source.BoardId}", source)
    {
        protected override string UrlComposite(int page)
        {
            return $"{UrlBase}&page={page}";
        }

        protected override void OnPageCrawl(IDocument document)
        {
            var cultureInfo = (CultureInfo)Thread.CurrentThread.CurrentCulture.Clone();
            var calendar = cultureInfo.Calendar;
            calendar.TwoDigitYearMax = DateTime.Now.Year + 30;
            cultureInfo.DateTimeFormat.Calendar = calendar;
            
            var thContent = document.QuerySelectorAll("thead tr th")
                .Select(x => x.TextContent.Trim())
                .ToList();
                
            var tdContent = document.QuerySelectorAll("table tbody tr")
                .Where(x => x.ClassName?.Contains("us-post") ?? false)
                .SelectMany(x => x.QuerySelectorAll("td")
                    .Select(cell =>
                    {
                        var cleanHtml = Regex.Replace(cell.InnerHtml, @"<span[^>]*?num_reply[^>]*?>.*?</span>", "", RegexOptions.IgnoreCase);
                        var textContent = Regex.Replace(cleanHtml, @"<[^>]+>", "").Trim();
                        return string.IsNullOrWhiteSpace(textContent) ? "" : textContent;
                    }))
                .ToArray();

            var tdHref = document.QuerySelectorAll("tbody tr")
                .Select(x => x.QuerySelectorAll("td"))
                .SelectMany(x => x.Where(y => y.QuerySelector("a") != null)
                                  .Select(y => y.QuerySelector("a").GetAttribute("href")))
                .Where(x => !string.IsNullOrEmpty(x) && x.Contains("board/view"))
                .ToArray();

            if (!thContent.Any() || !tdContent.Any())
            {
                Log.Error("Parsing Failed DOM. Not has thContent or tdContent {UrlComposite}", UrlComposite(1));
                return;
            }

            Parallel.For(0, tdContent.Length / thContent.Count, n =>
            {
                var cursor = n * thContent.Count;
                var id = tdContent.GetValue(thContent, "번호", cursor).ToIntRegex();
                var category = tdContent.GetValue(thContent, "말머리", cursor);
                var title = tdContent.GetValue(thContent, "제목", cursor).Substring("\n");
                var author = tdContent.GetValue(thContent, "글쓴이", cursor);
                var recommend = tdContent.GetValue(thContent, "추천", cursor).ToIntNullable();
                var count = tdContent.GetValue(thContent, "조회", cursor).ToInt();

                var dateTimeStr = tdContent.GetValue(thContent, "작성일", cursor);
                var date = ParseDate(dateTimeStr);

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
        
        static DateTime? ParseDate(string dateTimeStr)
        {
            CultureInfo cultureInfo = CultureInfo.InvariantCulture;
            int currentYear = DateTime.Now.Year;
            DateTime today = DateTime.Today;

            if (dateTimeStr.Contains('.'))
            {
                return dateTimeStr.Split('.').Length >= 3
                    ? DateTime.ParseExact(dateTimeStr, "yy.MM.dd", cultureInfo)
                    : new DateTime(currentYear, int.Parse(dateTimeStr.Split('.')[0]), int.Parse(dateTimeStr.Split('.')[1]));
            }
            else if (dateTimeStr.Contains(':'))
            {
                TimeSpan time = TimeSpan.Parse(dateTimeStr);
                return today.Add(time);
            }
            else
            {
                return DateTime.Parse(dateTimeStr);
            }
        }
    }
}
