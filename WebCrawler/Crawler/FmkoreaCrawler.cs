using EzAspDotNet.Util;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebCrawler.Models;

namespace WebCrawler.Crawler
{
    public class FmkoreaCrawler : CrawlerBase
    {
        public FmkoreaCrawler(CrawlDataDelegate onCrawlDataDelegate, IMongoDatabase mongoDb, Source source) :
            base(onCrawlDataDelegate, mongoDb, $"https://www.fmkorea.com/index.php", source)
        {
        }

        protected override string UrlComposite(int page)
        {
            return $"{UrlBase}?mid={Source.BoardId}&page={page}";
        }

        protected override void OnPageCrawl(AngleSharp.Html.Dom.IHtmlDocument document)
        {
            var thContent = document.QuerySelectorAll("thead tr th").Select(x => x.TextContent.Trim()).ToArray();
            if (thContent.Any())
            {
                OnPageCrawlTable(document, thContent);
            }
            else
            {
                OnPageCrawlList(document);
            }
        }

        protected override bool CanTwice() => false;

        private void OnPageCrawlTable(AngleSharp.Html.Dom.IHtmlDocument document, string[] thContent)
        {
            var tdAll = document.QuerySelectorAll("tbody tr td")
                .Where(x => !string.IsNullOrEmpty(x.ClassName) && !x.ClassName.Contains("notice"));

            var tdContent = tdAll.Select(x => x.TextContent.Trim()).ToArray();

            var tdHref = tdAll.Where(x => x.ClassName.Contains("title"))
                .Select(x => x.QuerySelector("a").GetAttribute("href")).ToArray();

            if (!thContent.Any() || !tdContent.Any())
            {
                return;
            }

            Parallel.For(0, tdContent.Length / thContent.Length, n =>
            {
                var cursor = n * thContent.Length;
                var category = tdContent[cursor + 0];
                var title = tdContent[cursor + 1];
                var author = tdContent[cursor + 2];
                var date = DateTime.Parse(tdContent[cursor + 3]);
                var count = tdContent[cursor + 4].ToInt();
                var recommend = tdContent[cursor + 5].ToInt();

                var href = UrlCompositeHref(tdHref[n]);

                _ = OnCrawlData(new CrawlingData
                {
                    Type = Source.Type,
                    BoardId = Source.BoardId,
                    BoardName = Source.Name,
                    Category = category,
                    Title = title.Substring("\t"),
                    Author = author,
                    Recommend = recommend,
                    Count = count,
                    DateTime = date,
                    Href = href,
                    SourceId = Source.Id
                });
            });
        }

        private void OnPageCrawlList(AngleSharp.Html.Dom.IHtmlDocument document)
        {
            var tdContent = document.QuerySelectorAll("ul li div")
                .Where(x => !string.IsNullOrEmpty(x.ClassName) && x.ClassName.Contains("li"))
                .Select(x =>
                {
                    var tuples = x.QuerySelectorAll("h3")
                        .Select(y =>
                        {
                            var textContent = y.TextContent.Trim();

                            var lastBracket = textContent.LastIndexOf("[");
                            if (lastBracket != -1)
                            {
                                textContent = textContent.Substring(0, lastBracket);
                            }

                            return new Tuple<string, string>("title", textContent);
                        }).ToList();

                    tuples.AddRange(x.QuerySelectorAll("span")
                        .Select(y => new Tuple<string, string>(y.ClassName, y.TextContent.Trim()))
                        .ToList());

                    tuples.AddRange(x.QuerySelectorAll("div")
                        .Where(y => !string.IsNullOrEmpty(y.ClassName) && y.ClassName == "hotdeal_info")
                        .Select(y => new Tuple<string, string>("info", y.TextContent.Replace("\t", string.Empty)))
                        .ToList());

                    var hrefs = x.QuerySelectorAll("a")
                        .Select(x => x.GetAttribute("href"))
                        .ToList();

                    return new Tuple<List<Tuple<string, string>>, List<string>>(tuples, hrefs);
                }).ToArray();

            Parallel.ForEach(tdContent, row =>
            {
                var stringTuples = row.Item1;
                var hrefs = row.Item2;

                var category = stringTuples.FindValue("category").Replace(" /", string.Empty);
                var title = stringTuples.FindValue("title").TrimEnd();

                var info = stringTuples.FindValue("info");
                if (!string.IsNullOrEmpty(info))
                {
                    title += $" [{info}]";
                }

                var author = stringTuples.FindValue("author").Replace("/ ", string.Empty);
                var date = DateTime.Now;
                var recommend = stringTuples.FindValue("count").ToIntRegex();

                var href = UrlCompositeHref(hrefs[0]);

                _ = OnCrawlData(new CrawlingData
                {
                    Type = Source.Type,
                    BoardId = Source.BoardId,
                    BoardName = Source.Name,
                    Category = category,
                    Title = title.Substring("\t"),
                    Author = author,
                    Recommend = recommend,
                    DateTime = date,
                    Href = href,
                    SourceId = Source.Id
                });
            });
        }
    }
}
