using EzAspDotNet.Util;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebCrawler.Models;

namespace WebCrawler.Crawler
{
    public class ItcmCrawler(CrawlDataDelegate onCrawlDataDelegate, IMongoDatabase mongoDb, Source source)
        : CrawlerBase(onCrawlDataDelegate, mongoDb, $"http://itcm.co.kr/index.php?mid=", source)
    {
        protected override string UrlComposite(int page)
        {
            return $"{UrlBase}{Source.BoardId}&page={page}";
        }

        protected override void OnPageCrawl(AngleSharp.Html.Dom.IHtmlDocument document)
        {
            var tdContents = document.QuerySelectorAll("tbody tr")
                .Where(x => string.IsNullOrEmpty(x.ClassName) || x.ClassName != "notice")
                .Select(x =>
                {
                    var stringTuples = x.QuerySelectorAll("td")
                         .Select(y =>
                         {
                             var text = y.TextContent.Trim();
                             if (string.IsNullOrEmpty(text))
                             {
                                 text = y.QuerySelectorAll("img")
                                    .Where(x => x.GetAttribute("src") != null)
                                    .Select(x => x.GetAttribute("title")).LastOrDefault();

                                 // LastOrDefault인 이유는 Author 부분이 First쪽이 레벨이기 때문
                             }

                             return new Tuple<string, string>(y.ClassName, text);
                         }).ToList();

                    var hrefs = x.QuerySelectorAll("a")
                        .Select(x => x.GetAttribute("href"))
                        .ToList();

                    return new Tuple<List<Tuple<string, string>>, List<string>>(stringTuples, hrefs);
                })
                .ToArray();

            Parallel.ForEach(tdContents, row =>
            {
                var stringTuples = row.Item1;
                var hrefs = row.Item2;

                var title = stringTuples.FindValue("title");
                title = title.Substring("\n");

                var category = stringTuples.FindValue("cate");
                var author = stringTuples.FindValue("author");
                var date = DateTime.Parse(stringTuples.FindValue("time"));
                if (date > DateTime.Now) // 12월 표기가 12-15라서 파싱되서 연초에는 꼬여서 예외처리
                {
                    date = date.AddYears(-1);
                }

                var count = stringTuples[7].Item2.ToInt();
                var recommend = stringTuples[8].Item2.ToInt();

                var href = hrefs[0];

                _ = OnCrawlData(new CrawlingData
                {
                    Type = Source.Type,
                    BoardId = Source.BoardId,
                    BoardName = Source.Name,
                    Title = title,
                    Category = category,
                    Author = author,
                    Count = count,
                    Recommend = recommend,
                    DateTime = date,
                    Href = href,
                    SourceId = Source.Id
                });
            });
        }
    }
}
