using System.Threading.Tasks;
using MongoDbWebUtil.Services;
using MongoDB.Driver;
using WebCrawler.Code;
using FeedCrawler.Models;
using FeedCrawler.Crawler;
using System;
using EzAspDotNet.Services;
using EzAspDotNet.Notification.Models;
using Server.Models;
using MongoDbWebUtil.Util;
using System.Collections.Generic;
using EzAspDotNet.Util;

namespace Server.Services
{
    public class FeedCrawlingService
    {
        private readonly MongoDbService _mongoDbService;

        private readonly RssService _rssService;

        private readonly WebHookService _webHookService;

        private readonly MongoDbUtil<FeedData> _mongoFeedData;

        public FeedCrawlingService(MongoDbService mongoDbService,
            RssService rssService,
            WebHookService webHookService)
        {
            _mongoDbService = mongoDbService;
            _rssService = rssService;
            _webHookService = webHookService;
            _mongoFeedData = new MongoDbUtil<FeedData>(mongoDbService.Database);

            _mongoFeedData.Collection.Indexes.CreateMany(new List<CreateIndexModel<FeedData>>
            {
                new CreateIndexModel<FeedData>(Builders<FeedData>.IndexKeys.Ascending(x => x.DateTime)),
                new CreateIndexModel<FeedData>(Builders<FeedData>.IndexKeys.Ascending(x => x.ItemTitle)),
                new CreateIndexModel<FeedData>(Builders<FeedData>.IndexKeys.Ascending(x => x.FeedTitle)),
                new CreateIndexModel<FeedData>(Builders<FeedData>.IndexKeys.Ascending(x => x.Url).Ascending(x => x.Href), 
                                               new CreateIndexOptions { Unique = true})
            });
        }

        public async Task<Protocols.Response.FeedList> Get(Protocols.Request.FeedList feedList)
        {
            var builder = Builders<FeedData>.Filter;
            var filter = FilterDefinition<FeedData>.Empty;
            if (!string.IsNullOrEmpty(feedList.Keyword))
            {
                filter &= builder.Regex(x => x.FeedTitle, "^" + feedList.Keyword + ".*");
            }

            return new Protocols.Response.FeedList
            {
                ResultCode = EzAspDotNet.Code.ResultCode.Success,
                Limit = feedList.Limit,
                Offset = feedList.Offset,
                Sort = feedList.Sort,
                Asc = feedList.Asc,
                Datas = (await _mongoFeedData.Page(filter, feedList.Limit, feedList.Offset, feedList.Sort, feedList.Asc)).ConvertAll(x => x.ToProtocol()),
                Total = await _mongoFeedData.CountAsync(filter)
            };
        }

        public async Task<Protocols.Response.Feed> Execute(Protocols.Request.Feed feed)
        {
            var onCrawlDataDelegate = new RssCrawler.CrawlDataDelegate(OnNewCrawlData);
            var rssList = feed.All ? (await _rssService.All()).ConvertAll(x => x.ToProtocol()) : feed.RssList;
            Parallel.ForEach(rssList, new ParallelOptions { MaxDegreeOfParallelism = 16 },
                async rss =>
                {
                    var update = await new RssCrawler(onCrawlDataDelegate, _mongoDbService.Database, rss.ToModel()).RunAsync();
                    if (update != null)
                    {
                        await _rssService.Update(update);
                    }
                }
            );

            return new Protocols.Response.Feed
            {
                ResultCode = EzAspDotNet.Code.ResultCode.Success
            };
        }

        public async Task OnNewCrawlData(FeedData feedData)
        {
            if(DateTime.Now.Subtract(feedData.DateTime).TotalDays > 7)
            {
                return;
            }

            await _webHookService.Execute(Builders<Notification>.Filter.Eq(x => x.CrawlingType, CrawlingType.Rss.ToString()),
                new EzAspDotNet.Notification.Data.WebHook
                {
                    Title = feedData.ItemTitle,
                    Text = feedData.FeedTitle,
                    Footer = feedData.ItemAuthor,
                    TitleLink = feedData.Href,
                    TimeStamp = feedData.DateTime.ToTimeStamp()
                });
        }
    }
}
