using AngleSharp;
using EzAspDotNet.Util;
using MongoDB.Driver;
using Serilog;
using System;
using System.Linq;
using System.Threading.Tasks;
using AngleSharp.Dom;
using WebCrawler.Models;

namespace WebCrawler.Crawler
{
    public class HumorUnivCrawler(CrawlDataDelegate onCrawlDataDelegate, IMongoDatabase mongoDb, Source source)
        : CrawlerBase(onCrawlDataDelegate, mongoDb, $"http://web.humoruniv.com/board/humor/list.html?table=", source)
    {
        protected override string UrlComposite(int page)
        {
            return page <= 1 ? $"{UrlBase}{Source.BoardId}" : $"{UrlBase}{Source.BoardId}&pg={page - 1}";
        }

        protected override string UrlCompositeHref(string href)
        {
            return UrlBase.CutAndComposite("/", 0, 5, "/" + href);
        }

        protected override void OnPageCrawl(IDocument document)
        {
            if (document.Head.QuerySelector("meta")?.GetAttribute("http-equiv") == "refresh")
            {
                Log.Error($"http-equip refresh. {UrlComposite(1)}.\n<html:{document.ToHtml()}>");
                return;
            }

            var tdContent = document.QuerySelectorAll("tr td")
                .Where(x => !string.IsNullOrEmpty(x.ClassName) && x.ClassName.StartsWith("li_"))
                .ToArray();

            if (!tdContent.Any())
            {
                Log.Error($"Parsing Failed DOM. Not has tdContent {UrlComposite(1)}.\n<html:{document.ToHtml()}>");
                return;
            }

            const int thLength = 7;
            var thContent = tdContent.Take(thLength);
            tdContent = tdContent.Skip(thLength).ToArray();

            Parallel.For(0, tdContent.Length / thLength, n =>
            {
                var cursor = n * thLength;

                var originTitle = tdContent[cursor + 1].QuerySelector("a").TextContent.Trim();

                var title = originTitle.Substring("\n");
                var author = tdContent[cursor + 2].TextContent.Trim();
                var date = DateTime.Parse(tdContent[cursor + 3].TextContent.Trim());
                var count = tdContent[cursor + 4].TextContent.Trim().ToInt();
                var recommend = tdContent[cursor + 5].TextContent.Trim().ToInt();
                var notRecommend = tdContent[cursor + 6].TextContent.Trim().ToInt();

                var href = UrlCompositeHref(tdContent[cursor + 1].QuerySelector("a").GetAttribute("href"));

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
