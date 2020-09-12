using MongoDB.Driver;
using System;
using System.Linq;
using System.Threading.Tasks;
using WebCrawler.Code;
using WebCrawler.Models;
using WebUtil.Util;

namespace WebCrawler
{
    public class RuliwebCrawler : CrawlerBase
    {
        public RuliwebCrawler(IMongoDatabase mongoDb, int boardId = 1020 /* hotdeal */, int page = 1) :
            base(mongoDb, $"https://bbs.ruliweb.com/market/board/{boardId}", boardId.ToString(), page)
        {
        }

        protected override string UrlComposite(int page)
        {
            return $"{UrlBase}?page={page}";
        }

        protected override void OnPageCrawl(AngleSharp.Html.Dom.IHtmlDocument document)
        {
            var thContent = document.QuerySelectorAll("thead tr th").Select(x => x.TextContent.Trim()).ToArray();
            var tdContent = document.QuerySelectorAll("tbody tr td").Select(x => x.TextContent.Trim()).ToArray();
            var tdHref = document.QuerySelectorAll("tbody tr td a").Where(x => x.ClassName == "deco").Select(x => x.GetAttribute("href")).ToArray();

            Parallel.For(0, tdContent.Length / thContent.Length, n =>
            {
                var cursor = n * thContent.Length;
                var id = tdContent[cursor + 0].ToIntNullable();
                var category = tdContent[cursor + 1];
                var title = tdContent[cursor + 2];
                var author = tdContent[cursor + 3];
                var recommend = tdContent[cursor + 4].ToInt();
                var count = tdContent[cursor + 5].ToInt();
                var date = DateTime.Parse(tdContent[cursor + 6]);

                var href = tdHref[n];

                _ = OnCrawlData(new CrawlingData
                {
                    Type = CrawlingType.Ruliweb,
                    BoardId = BoardId,
                    Category = category,
                    Title = title,
                    Author = author,
                    Recommend = recommend,
                    Count = count,
                    DateTime = date,
                    RowId = id,
                    Href = href
                });
            });
        }
    }
}
