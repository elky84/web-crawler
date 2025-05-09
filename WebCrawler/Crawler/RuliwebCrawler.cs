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
    public partial class RuliwebCrawler(CrawlDataDelegate onCrawlDataDelegate, IMongoDatabase mongoDb, Source source)
        : CrawlerBase(onCrawlDataDelegate, mongoDb, $"https://bbs.ruliweb.com/{source.BoardId}", source)
    {
        protected override string UrlComposite(int page)
        {
            return $"{UrlBase}?page={page}";
        }

        protected override void OnPageCrawl(IDocument document)
        {
            var cultureInfo = (CultureInfo)Thread.CurrentThread.CurrentCulture.Clone();
            var calendar = cultureInfo.Calendar;
            calendar.TwoDigitYearMax = DateTime.Now.Year + 30;
            cultureInfo.DateTimeFormat.Calendar = calendar;

            var bestBody = document.GetElementById("best_body");
            if (bestBody != null)
            {
                var rows = bestBody.QuerySelectorAll("tr")
                    .Where(x => x.ClassName.Contains("table_body"))
                    .Select(x => x.TextContent.Trim())
                    .ToList();

                var tdContent = bestBody.QuerySelectorAll("td")
                    .Select(cell =>
                    {
                        var cleanHtml = Regex.Replace(cell.InnerHtml, 
                            @"<span[^>]*(class\s*=\s*[""'][^""']*num_reply[^""']*[""']|style\s*=\s*[""'][^""']*(width\s*:\s*20px;).*?[""'])[^>]*?>.*?</span>", 
                            "", 
                            RegexOptions.IgnoreCase | RegexOptions.Singleline);

                        // 태그를 제외한 순수 텍스트 추출
                        var textContent = Regex.Replace(cleanHtml, @"<[^>]+>", "").Trim();

                        return string.IsNullOrWhiteSpace(textContent) ? "" : textContent;
                    })
                    .ToArray();

                var tdHref = document.QuerySelectorAll("tbody tr")
                    .Select(x => x.QuerySelectorAll("td"))
                    .SelectMany(x => x.Where(y => y.QuerySelector("a") != null)
                        .Select(y => y.QuerySelector("a").GetAttribute("href")))
                    .ToArray();
                
                if (rows.Count == 0 || tdContent.Length == 0)
                {
                    Log.Error("Parsing Failed DOM. Not has rows or tdContent {UrlComposite}", UrlComposite(1));
                    return;
                }

                var colCount = tdContent.Length / rows.Count;

                for(var n = 0; n < rows.Count; ++n)
                {
                    var cursor = n * colCount;
                    var id = tdContent[cursor + 0].ToIntRegex();

                    var title = tdContent[cursor + 1];

                    var author = tdContent[cursor + 2];
                    var recommend = tdContent[cursor + 3].ToIntRegex();
                    var count = tdContent[cursor + 4].ToIntRegex();

                    var dateTimeStr = tdContent[cursor + 5].Replace("날짜 ", string.Empty);
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

                    var href = tdHref[n];

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
                        DateTime = date.GetValueOrDefault(DateTime.Now),
                        Href = Host + href,
                        SourceId = Source.Id
                    });
                }
            }
            else
            {
                var thContent = document.QuerySelectorAll("thead tr th")
                .Select(x => x.TextContent.Trim())
                .ToList();
                
                var tdContent = document.QuerySelectorAll("tbody tr")
                    .Where(x => x.ClassName.Contains("table_body") && x.ClassName.Contains("blocktarget") && !string.IsNullOrEmpty(x.TextContent))
                    .SelectMany(x => x.QuerySelectorAll("td")
                        .Select(cell =>
                        {
                            // InnerHtml에서 <span class="num_reply">...</span> 제거
                            var cleanHtml = Regex.Replace(cell.InnerHtml, @"<span[^>]*?num_reply[^>]*?>.*?</span>", "", RegexOptions.IgnoreCase);

                            // 태그를 제외한 순수 텍스트 추출
                            var textContent = Regex.Replace(cleanHtml, @"<[^>]+>", "").Trim();

                            return string.IsNullOrWhiteSpace(textContent) ? "" : textContent;
                        }))
                    .ToArray();

                var tdHref = document.QuerySelectorAll("tbody tr")
                    .Where(x => x.ClassName.Contains("table_body") && x.ClassName.Contains("blocktarget"))
                    .Select(x => x.QuerySelectorAll("td"))
                    .SelectMany(x => x.Where(y => y.ClassName == "subject" && y.QuerySelector("a") != null)
                                      .Select(y => y.QuerySelector("a").GetAttribute("href")))
                    .Where(x => x.StartsWith("http"))
                    .ToArray();

                if (thContent.Count == 0 || tdContent.Length == 0)
                {
                    Log.Error("Parsing Failed DOM. Not has thContent or tdContent {UrlComposite}", UrlComposite(1));
                    return;
                }

                for(var n = 0; n < tdContent.Length / thContent.Count; ++n)
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
                }
            }
        }
    }
}
