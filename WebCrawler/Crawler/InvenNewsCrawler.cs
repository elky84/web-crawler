using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WebCrawler.Models;
using WebUtil.Util;

namespace WebCrawler.Crawler
{
    public class InvenNewsCrawler : CrawlerBase
    {
        public InvenNewsCrawler(CrawlDataDelegate onCrawlDataDelegate, IMongoDatabase mongoDb, Source source) :
            base(onCrawlDataDelegate, mongoDb, $"http://www.inven.co.kr/webzine/news/", source)
        {
        }

        protected override string UrlComposite(int page)
        {
            return $"{UrlBase}?{Source.BoardId}&page={page}";
        }

        protected override void OnPageCrawl(AngleSharp.Html.Dom.IHtmlDocument document)
        {
            var tdContent = document.QuerySelectorAll("tbody tr")
                .Select(x =>
                {
                    var stringTuples = x.QuerySelectorAll("span")
                        .Where(x => x.ClassName != "category")
                        .Select(y => new Tuple<string, string>(y.ClassName, y.TextContent.Trim())).ToList();

                    var hrefs = x.QuerySelectorAll("a")
                        .Select(x => x.GetAttribute("href"))
                        .ToList();

                    return new Tuple<List<Tuple<string, string>>, List<string>>(stringTuples, hrefs);
                })
                .ToArray();

            Parallel.ForEach(tdContent, row =>
            {
                var stringTuples = row.Item1;
                var hrefs = row.Item2;

                var cmtnum = stringTuples.FindValue("cmtnum");

                var originTitle = stringTuples.FindValue("title");

                var infos = stringTuples.FindValue("info").Split("|");
                var title = string.IsNullOrEmpty(cmtnum) ? originTitle : originTitle.Substring(cmtnum);
                var category = infos[0];
                var author = infos[1];
                var date = DateTime.Parse(infos[2]);
                var recommend = string.IsNullOrEmpty(cmtnum) ? 0 : cmtnum.ToIntRegx();

                var href = hrefs[0];

                _ = OnCrawlData(new CrawlingData
                {
                    Type = Source.Type,
                    BoardId = Source.BoardId,
                    BoardName = Source.Name,
                    Title = title,
                    Category = category,
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
