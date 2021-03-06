﻿using MongoDB.Driver;
using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WebCrawler.Models;
using WebUtil.Util;

namespace WebCrawler.Crawler
{
    public class RuliwebCrawler : CrawlerBase
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
            var thContent = document.QuerySelectorAll("thead tr th")
                .Select(x => x.TextContent.Trim())
                .ToList();

            var tdContent = document.QuerySelectorAll("tbody tr")
                .Where(x => x.ClassName == "table_body")
                .Select(x => x.QuerySelectorAll("td"))
                .SelectMany(x => x.Select(y => y.TextContent.Trim()))
                .ToArray();

            var tdHref = document.QuerySelectorAll("tbody tr")
                .Where(x => x.ClassName == "table_body")
                .Select(x => x.QuerySelectorAll("td"))
                .SelectMany(x => x.Where(y => y.ClassName == "subject" && y.QuerySelector("a") != null)
                                  .Select(y => y.QuerySelector("a").GetAttribute("href")))
                .Where(x => x.StartsWith("http"))
                .ToArray();

            if (!thContent.Any() || !tdContent.Any())
            {
                return;
            }

            var cultureInfo = (CultureInfo)Thread.CurrentThread.CurrentCulture.Clone();
            var calendar = cultureInfo.Calendar;
            calendar.TwoDigitYearMax = DateTime.Now.Year + 30;
            cultureInfo.DateTimeFormat.Calendar = calendar;

            Parallel.For(0, tdContent.Length / thContent.Count, n =>
            {
                var cursor = n * thContent.Count;
                var id = tdContent.GetValue(thContent, "ID", cursor).ToIntNullable();
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

                var href = tdHref[n];

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
}
