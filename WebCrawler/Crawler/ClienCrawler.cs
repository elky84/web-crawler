using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebCrawler.Code;
using WebCrawler.Models;
using WebUtil.Util;

namespace WebCrawler
{
    public class ClienCrawler : CrawlerBase
    {
        public ClienCrawler(IMongoDatabase mongoDb, Source source) :
            base(mongoDb, $"https://www.clien.net/service/board/{source.BoardId}", source)
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
                    var stringTuples = x.QuerySelectorAll("span").Select(y => new Tuple<string, string>(y.ClassName, y.TextContent.Trim())).ToList();
                    var hrefs = x.QuerySelectorAll("a").Select(x => x.GetAttribute("href")).ToList();
                    return new Tuple<List<Tuple<string, string>>, List<string>>(stringTuples, hrefs);
                })
                .ToArray();

            Parallel.ForEach(tdContent, row =>
            {
                var stringTuples = row.Item1;
                var hrefs = row.Item2;

                var category = stringTuples.FindValue("category_fixed");
                var title = stringTuples.FindValue("subject_fixed");
                var author = stringTuples.FindValue("nickname");
                var count = stringTuples.FindValue("hit").ToInt();
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
                    Href = href
                });
            });
        }
    }
}
