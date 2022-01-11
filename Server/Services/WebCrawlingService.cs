using EzAspDotNet.Util;
using System.Threading.Tasks;
using MongoDbWebUtil.Services;
using WebCrawler;
using MongoDB.Driver;
using WebCrawler.Code;
using WebCrawler.Models;
using WebCrawler.Crawler;
using EzAspDotNet.Services;
using Server.Models;
using EzAspDotNet.Notification.Models;
using EzAspDotNet.Exception;
using MongoDbWebUtil.Util;

namespace Server.Services
{
    public class WebCrawlingService
    {
        private readonly MongoDbService _mongoDbService;

        private readonly SourceService _sourceService;

        private readonly WebHookService _webHookService;

        private readonly MongoDbUtil<CrawlingData> _mongoCrawlingData;

        public WebCrawlingService(MongoDbService mongoDbService,
            SourceService sourceService,
            WebHookService webHookService)
        {
            _mongoDbService = mongoDbService;
            _sourceService = sourceService;
            _webHookService = webHookService;
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
                ResultCode = EzAspDotNet.Code.ResultCode.Success,
                Limit = crawlingList.Limit,
                Offset = crawlingList.Offset,
                Sort = crawlingList.Sort,
                Asc = crawlingList.Asc,
                Datas = (await _mongoCrawlingData.Page(filter, crawlingList.Limit, crawlingList.Offset, crawlingList.Sort, crawlingList.Asc)).ConvertAll(x => x.ToProtocol()),
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
                        case CrawlingType.InvenNews:
                            await new InvenNewsCrawler(onCrawlDataDelegate, _mongoDbService.Database, source.ToModel()).RunAsync();
                            break;
                        case CrawlingType.HumorUniv:
                            await new HumorUnivCrawler(onCrawlDataDelegate, _mongoDbService.Database, source.ToModel()).RunAsync();
                            break;
                        case CrawlingType.Itcm:
                            await new ItcmCrawler(onCrawlDataDelegate, _mongoDbService.Database, source.ToModel()).RunAsync();
                            break;
                        default:
                            throw new DeveloperException(EzAspDotNet.Code.ResultCode.NotImplementedYet);
                    }
                }
            );

            return new Protocols.Response.Crawling
            {
                ResultCode = EzAspDotNet.Code.ResultCode.Success
            };
        }

        public async Task OnNewCrawlData(CrawlingData crawlingData)
        {
            var category = string.IsNullOrEmpty(crawlingData.Category) ? string.Empty : $"[{crawlingData.Category}] ";

            // : 이 들어가면 slack desktop alarm 에서 표기 오류가 발생한다. 그래서 치환
            await _webHookService.Execute(Builders<Notification>.Filter.Eq(x => x.SourceId, crawlingData.SourceId), 
                crawlingData.Title,
                $"<{crawlingData.Href}|{category}{crawlingData.Title.Replace(":", ".")}>" +
                $" - {crawlingData.DateTime:yyyy.MM.dd HH.mm.dd}");
        }
    }
}
