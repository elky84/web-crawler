﻿using EzAspDotNet.Util;
using MongoDB.Driver;
using Serilog;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AngleSharp.Dom;
using WebCrawler.Models;

namespace WebCrawler.Crawler
{
    public class SlrclubCrawler(CrawlDataDelegate onCrawlDataDelegate, IMongoDatabase mongoDb, Source source)
        : CrawlerBase(onCrawlDataDelegate, mongoDb, $"http://www.slrclub.com/bbs/zboard.php", source)
    {
        private static int? LatestPage { get; set; }

        protected override string UrlComposite(int page)
        {
            return $"{UrlBase}?id={Source.BoardId}&page={LatestPage - page - 1}";
        }

        public override async Task RunAsync()
        {
            var pageInfoCrawler = new SlrclubPageInfoCrawler(null, null, Source);
            await pageInfoCrawler.RunAsync();

            if (SlrclubPageInfoCrawler.LatestPage.HasValue)
            {
                LatestPage = SlrclubPageInfoCrawler.LatestPage;
            }

            if (!LatestPage.HasValue)
            {
                Log.Logger.Error("Not Found PageInfo Data {UrlBase}", UrlBase);
                return;
            }

            for (var page = Source.PageMin; page <= Source.PageMax; ++page)
            {
                await ExecuteAsync(page);
                Thread.Sleep(Source.Interval);
            }
        }

        protected override void OnPageCrawl(IDocument document)
        {
            var thContent = document.QuerySelectorAll("thead tr th")
                .Select(x => x.TextContent.Trim()).ToArray();

            var tdContent = document.QuerySelectorAll("tbody tr td")
                .Select(x => x.QuerySelector("a") != null ? x.QuerySelector("a").TextContent.Trim() : x.TextContent.Trim())
                .ToArray();

            var tdHref = document.QuerySelectorAll("tbody tr td")
                .Where(x => !string.IsNullOrEmpty(x.ClassName) && x.ClassName.Contains("sbj"))
                .Select(x => x.QuerySelector("a").GetAttribute("href"))
                .ToArray();

            if (thContent.Length == 0 || tdContent.Length == 0)
            {
                Log.Error("Parsing Failed DOM. Not has thContent or tdContent {UrlComposite}", UrlComposite(1));
                return;
            }

            for(var n = 0; n < tdContent.Length / thContent.Length; ++n)
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
            }
        }
    }
}
