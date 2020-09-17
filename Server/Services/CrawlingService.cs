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

namespace Server.Services
{
    public class CrawlingService
    {
        private readonly MongoDbService _mongoDbService;

        private readonly SourceService _sourceService;

        private readonly NotificationService _notificationService;

        public CrawlingService(MongoDbService mongoDbService,
            SourceService sourceService,
            NotificationService notificationService)
        {
            _mongoDbService = mongoDbService;
            _sourceService = sourceService;
            _notificationService = notificationService;
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
