using WebUtil.Util;
using System.Linq;
using System.Threading.Tasks;
using WebUtil.Services;
using WebCrawler;
using MongoDB.Driver;
using WebCrawler.Code;
using Server.Exception;
using Server.Models;
using WebCrawler.Models;
using FeedCrawler.Models;
using FeedCrawler;

namespace Server.Services
{
    public class FeedCrawlingService
    {
        private readonly MongoDbService _mongoDbService;

        private readonly RssService _rssService;

        private readonly NotificationService _notificationService;

        private readonly MongoDbUtil<FeedData> _mongoFeedData;

        public FeedCrawlingService(MongoDbService mongoDbService,
            RssService rssService,
            NotificationService notificationService)
        {
            _mongoDbService = mongoDbService;
            _rssService = rssService;
            _notificationService = notificationService;
            _mongoFeedData = new MongoDbUtil<FeedData>(mongoDbService.Database);

            _mongoFeedData.Collection.Indexes.CreateOne(new CreateIndexModel<FeedData>(
                Builders<FeedData>.IndexKeys.Ascending(x => x.DateTime)));

            _mongoFeedData.Collection.Indexes.CreateOne(new CreateIndexModel<FeedData>(
                Builders<FeedData>.IndexKeys.Ascending(x => x.ItemTitle)));

            _mongoFeedData.Collection.Indexes.CreateOne(new CreateIndexModel<FeedData>(
                Builders<FeedData>.IndexKeys.Ascending(x => x.FeedTitle)));
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
                ResultCode = Code.ResultCode.Success,
                Limit = feedList.Limit,
                Offset = feedList.Offset,
                Sort = feedList.Sort,
                Asc = feedList.Asc,
                FeedDatas = (await _mongoFeedData.Page(filter, feedList.Limit, feedList.Offset, feedList.Sort, feedList.Asc)).ConvertAll(x => x.ToProtocol()),
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
                    await new RssCrawler(onCrawlDataDelegate, _mongoDbService.Database, rss.ToModel()).RunAsync();
                }
            );

            return new Protocols.Response.Feed
            {
                ResultCode = Code.ResultCode.Success
            };
        }

        public async Task OnNewCrawlData(FeedData feedData)
        {
            await _notificationService.Execute(Builders<Notification>.Filter.Eq(x => x.CrawlingType, CrawlingType.Rss), feedData);
        }
    }
}
