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
    public class TodayhumorCrawler : CrawlerBase
    {
        public TodayhumorCrawler(IMongoDatabase mongoDb, Source source) :
            base(mongoDb, $"http://www.todayhumor.co.kr/board/list.php", source)
        {
        }

        protected override string UrlComposite(int page)
        {
            return $"{UrlBase}?table={Source.BoardId}&page={page}";
        }

        protected override void OnPageCrawl(AngleSharp.Html.Dom.IHtmlDocument document)
        {
            var thContent = document.QuerySelectorAll("thead tr th")
                .Select(x => x.TextContent.Trim())
                .ToArray();

            var tdContent = document.QuerySelectorAll("tbody tr")
                .Where(x => x.ClassName == "view list_tr_humordata")
                .Select(x => x.QuerySelectorAll("td"))
                .SelectMany(x => x.Select(y => y.TextContent.Trim()))
                .ToArray();

            var tdHref = document.QuerySelectorAll("tbody tr td")
                .Where(x => x.ClassName == ("subject"))
                .Select(x => x.QuerySelector("a").GetAttribute("href"))
                .ToArray();

            Parallel.For(0, tdContent.Length / thContent.Length, n =>
            {
                var cursor = n * thContent.Length;
                var id = tdContent[cursor + 0].ToInt();
                var title = tdContent[cursor + 2];
                var author = tdContent[cursor + 3];
                var date = DateTime.Parse(tdContent[cursor + 4]);
                var count = tdContent[cursor + 5].ToInt();
                var recommend = tdContent[cursor + 6].ToInt();

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
                    Href = href
                });
            });
        }
    }
}
