using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebCrawler.Models;
using MongoDbWebUtil.Util;

namespace WebCrawler.Crawler
{
    public class ClienCrawler : CrawlerBase
    {
        public ClienCrawler(CrawlDataDelegate onCrawlDataDelegate, IMongoDatabase mongoDb, Source source) :
            base(onCrawlDataDelegate, mongoDb, $"https://www.clien.net/service/board/{source.BoardId}", source)
        {
        }

        protected override string UrlComposite(int page)
        {
            // 페이지가 0부터 시작함
            return $"{UrlBase}?od=T32&po={page - 1}";
        }

        protected override void OnPageCrawl(AngleSharp.Html.Dom.IHtmlDocument document)
        {
            var tdContent = document.QuerySelectorAll("div")
                .Where(x => !string.IsNullOrEmpty(x.ClassName) && x.ClassName.Contains("list_item") && x.ClassName.Contains("symph_row"))
                .Select(x =>
                {
                    var stringTuples = x.QuerySelectorAll("span")
                        .Select(y =>
                        {
                            var text = y.TextContent.Trim();
                            if (string.IsNullOrEmpty(text))
                            {
                                text = y.QuerySelector("img")?.GetAttribute("alt");
                            }

                            return new Tuple<string, string>(y.ClassName, text);
                        }).ToList();

                    var a = x.QuerySelectorAll("a");

                    stringTuples.AddRange(a.Where(x => !string.IsNullOrEmpty(x.ClassName))
                        .Select(y => new Tuple<string, string>(y.ClassName, y.TextContent))
                        .ToList());

                    var hrefs = a.Select(x => x.GetAttribute("href"))
                        .ToList();

                    return new Tuple<List<Tuple<string, string>>, List<string>>(stringTuples, hrefs);
                })
                .ToArray();

            Parallel.ForEach(tdContent, row =>
                {
                    var stringTuples = row.Item1;
                    var hrefs = row.Item2;

                    var category = stringTuples.FindValue("category_fixed");
                    if (string.IsNullOrEmpty(category))
                    {
                        category = stringTuples.FindValue("icon_keyword");
                    };

                    var title = stringTuples.FindValue("subject_fixed");
                    if (string.IsNullOrEmpty(title))
                    {
                        title = stringTuples.FindValue("list_subject");
                    };

                    title = title.Substring("\n");

                    var author = stringTuples.FindValue("nickname");
                    var count = stringTuples.FindValue("hit").ToIntShorthand();
                    var date = DateTime.Parse(stringTuples.FindValue("timestamp"));

                    var href = UrlCompositeHref(hrefs[0]);

                    _ = OnCrawlData(new CrawlingData
                    {
                        Type = Source.Type,
                        BoardId = Source.BoardId,
                        BoardName = Source.Name,
                        Category = category,
                        Title = title,
                        Author = author,
                        Count = count,
                        DateTime = date,
                        Href = href,
                        SourceId = Source.Id
                    });
                });
        }
    }
}
