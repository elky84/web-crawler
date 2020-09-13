﻿using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebCrawler.Code;
using WebCrawler.Models;
using WebUtil.Util;

namespace WebCrawler
{
    public class PpomppuCrawler : CrawlerBase
    {
        public PpomppuCrawler(IMongoDatabase mongoDb, Source source) :
            base(mongoDb, $"http://www.ppomppu.co.kr/zboard/zboard.php", source)
        {
        }

        protected override string UrlComposite(int page)
        {
            return $"{UrlBase}?id={Source.BoardId}&page={page}";
        }

        protected override void OnPageCrawl(AngleSharp.Html.Dom.IHtmlDocument document)
        {
            var thContent = document.QuerySelectorAll("tbody tr")
                .Where(x => x.ClassName == "title_bg")
                .Select(x => x.QuerySelectorAll("td").Where(x => x.ClassName == "list_tspace"))
                .SelectMany(x => x.Select(y => y.TextContent.Trim()))
                .ToArray();

            var tdContent = document.QuerySelectorAll("tbody tr")
                .Where(x => x.ClassName == "list0" || x.ClassName == "list1")
                .Select(x => x.QuerySelectorAll("td").Where(x => !string.IsNullOrEmpty(x.ClassName) && x.ClassName.Contains("list_vspace")))
                .SelectMany(x => x.Select(y => y.TextContent.Trim()))
                .ToArray();

            var tdHref = document.QuerySelectorAll("tbody tr")
                .Where(x => x.ClassName == "list0" || x.ClassName == "list1")
                .Select(x => x.QuerySelectorAll("td a"))
                .SelectMany(x => x.Select(y => y.GetAttribute("href")))
                .Where(x => x != "#")
                .ToArray();

            Parallel.For(0, tdContent.Length / thContent.Length, n =>
            {
                var cursor = n * thContent.Length;
                var id = tdContent[cursor + 0].ToInt();
                var author = tdContent[cursor + 1];
                var title = tdContent[cursor + 2];
                var date = DateTime.Parse(tdContent[cursor + 3]);

                var str = tdContent[cursor + 4];
                var recommend = string.IsNullOrEmpty(str) ? 0 : str.Split(" - ")[0].ToInt();

                var count = tdContent[cursor + 5].ToInt();

                var href = UrlCompositeHref("/" + tdHref[n]);

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
