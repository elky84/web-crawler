﻿using EzAspDotNet.Util;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AngleSharp.Dom;
using WebCrawler.Models;

namespace WebCrawler.Crawler
{
    public class InvenNewsCrawler(CrawlDataDelegate onCrawlDataDelegate, IMongoDatabase mongoDb, Source source)
        : CrawlerBase(onCrawlDataDelegate, mongoDb, $"http://www.inven.co.kr/webzine/news/", source)
    {
        protected override string UrlComposite(int page)
        {
            return $"{UrlBase}?{Source.BoardId}&page={page}";
        }

        protected override void OnPageCrawl(IDocument document)
        {
            var tdContents = document.QuerySelectorAll("tbody tr")
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

            foreach(var (stringTuples, hrefs) in tdContents)
            {
                var count = stringTuples.FindValue("cmtnum");

                var originTitle = stringTuples.FindValue("title");

                var infos = stringTuples.FindValue("info").Split("|");
                var title = string.IsNullOrEmpty(count) ? originTitle : originTitle.Substring(count);
                var category = infos[0].Substring("\n");
                var author = infos.Count() <= 2 ? "" : infos[1];
                var date = infos.Count() <= 2 ? DateTime.Parse(infos[1]) : DateTime.Parse(infos[2]);
                var recommend = string.IsNullOrEmpty(count) ? 0 : count.ToIntRegex();

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
            }
        }
    }
}
