using WebUtil.Util;
using System.Threading.Tasks;
using WebUtil.Services;
using WebCrawler;
using MongoDB.Driver;
using WebCrawler.Code;
using Server.Exception;
using Server.Models;
using WebCrawler.Models;
using WebCrawler.Crawler;

namespace Server.Services
{
    public class WebCrawlingService
    {
        private readonly MongoDbService _mongoDbService;

        private readonly SourceService _sourceService;

        private readonly NotificationService _notificationService;

        private readonly MongoDbUtil<CrawlingData> _mongoCrawlingData;

        public WebCrawlingService(MongoDbService mongoDbService,
            SourceService sourceService,
            NotificationService notificationService)
        {
            _mongoDbService = mongoDbService;
            _sourceService = sourceService;
            _notificationService = notificationService;
            _mongoCrawlingData = new MongoDbUtil<CrawlingData>(mongoDbService.Database);

            _mongoCrawlingData.Collection.Indexes.CreateOne(new CreateIndexModel<CrawlingData>(
                Builders<CrawlingData>.IndexKeys.Ascending(x => x.DateTime)));

            _mongoCrawlingData.Collection.Indexes.CreateOne(new CreateIndexModel<CrawlingData>(
                Builders<CrawlingData>.IndexKeys.Ascending(x => x.Title)));

            _mongoCrawlingData.Collection.Indexes.CreateOne(new CreateIndexModel<CrawlingData>(
                Builders<CrawlingData>.IndexKeys.Ascending(x => x.Type)));

            _mongoCrawlingData.Collection.Indexes.CreateOne(new CreateIndexModel<CrawlingData>(
                Builders<CrawlingData>.IndexKeys.Ascending(x => x.Type).Ascending(x => x.BoardId)));
        }

        public async Task<Protocols.Response.CrawlingList> Get(Protocols.Request.CrawlingList crawlingList)
        {
            var builder = Builders<CrawlingData>.Filter;
            var filter = FilterDefinition<CrawlingData>.Empty;
            if (!string.IsNullOrEmpty(crawlingList.Keyword))
            {
                filter &= builder.Regex(x => x.Title, ".*" + crawlingList.Keyword + ".*");
            }

            if (crawlingList.Type.HasValue)
            {
                filter &= builder.Eq(x => x.Type, crawlingList.Type.Value);
            }

            return new Protocols.Response.CrawlingList
            {
                ResultCode = Code.ResultCode.Success,
                Limit = crawlingList.Limit,
                Offset = crawlingList.Offset,
                Sort = crawlingList.Sort,
                Asc = crawlingList.Asc,
                CrawlingDatas = (await _mongoCrawlingData.Page(filter, crawlingList.Limit, crawlingList.Offset, crawlingList.Sort, crawlingList.Asc)).ConvertAll(x => x.ToProtocol()),
                Total = await _mongoCrawlingData.CountAsync(filter)
            };
        }

        public async Task<Protocols.Response.Crawling> Execute(Protocols.Request.Crawling crawling)
        {
            var onCrawlDataDelegate = new CrawlerBase.CrawlDataDelegate(OnNewCrawlData);
            var sources = crawling.All ? (await _sourceService.All()).ConvertAll(x => x.ToProtocol()) : crawling.Sources;
            Parallel.ForEach(sources, new ParallelOptions { MaxDegreeOfParallelism = 16 },
                async source =>
                {
                    switch (source.Type)
                    {
                        case CrawlingType.Ruliweb:
                            await new RuliwebCrawler(onCrawlDataDelegate, _mongoDbService.Database, source.ToModel()).RunAsync();
                            break;
                        case CrawlingType.Clien:
                            await new ClienCrawler(onCrawlDataDelegate, _mongoDbService.Database, source.ToModel()).RunAsync();
                            break;
                        case CrawlingType.SlrClub:
                            await new SlrclubCrawler(onCrawlDataDelegate, _mongoDbService.Database, source.ToModel()).RunAsync();
                            break;
                        case CrawlingType.Ppomppu:
                            await new PpomppuCrawler(onCrawlDataDelegate, _mongoDbService.Database, source.ToModel()).RunAsync();
                            break;
                        case CrawlingType.TodayHumor:
                            await new TodayhumorCrawler(onCrawlDataDelegate, _mongoDbService.Database, source.ToModel()).RunAsync();
                            break;
                        case CrawlingType.FmKorea:
                            await new FmkoreaCrawler(onCrawlDataDelegate, _mongoDbService.Database, source.ToModel()).RunAsync();
                            break;
                        default:
                            throw new DeveloperException(Code.ResultCode.NotImplementedYet);
                    }
                }
            );

            return new Protocols.Response.Crawling
            {
                ResultCode = Code.ResultCode.Success
            };
        }

        public async Task OnNewCrawlData(CrawlingData crawlingData)
        {
            await _notificationService.Execute(Builders<Notification>.Filter.Eq(x => x.SourceId, crawlingData.SourceId), crawlingData);
        }
    }
}
