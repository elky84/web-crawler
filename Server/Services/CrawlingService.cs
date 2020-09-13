using WebUtil.Util;
using System.Linq;
using System.Threading.Tasks;
using WebUtil.Services;
using WebCrawler;
using MongoDB.Driver;
using WebCrawler.Code;
using Server.Exception;
using Server.Models;

namespace Server.Services
{
    public class CrawlingService
    {
        private readonly MongoDbService _mongoDbService;

        private readonly SourceService _sourceService;

        public CrawlingService(MongoDbService mongoDbService,
            SourceService sourceService)
        {
            _mongoDbService = mongoDbService;
            _sourceService = sourceService;
        }

        public async Task<Protocols.Response.Crawling> Execute(Protocols.Request.Crawling crawling)
        {
            var sources = crawling.All ? (await _sourceService.All()).ConvertAll(x => x.ToProtocol()) : crawling.Sources;
            Parallel.ForEach(sources, new ParallelOptions { MaxDegreeOfParallelism = 16 },
                async source =>
                {
                    switch (source.Type)
                    {
                        case CrawlingType.Ruliweb:
                            await WhenAll(crawling.Page, new RuliwebCrawler(_mongoDbService.Database, source.ToModel()));
                            break;
                        case CrawlingType.Clien:
                            await WhenAll(crawling.Page, new ClienCrawler(_mongoDbService.Database, source.ToModel()));
                            break;
                        case CrawlingType.SlrClub:
                            await WhenAll(crawling.Page, new SlrclubCrawler(_mongoDbService.Database, source.ToModel()));
                            break;
                        case CrawlingType.Ppomppu:
                            await WhenAll(crawling.Page, new PpomppuCrawler(_mongoDbService.Database, source.ToModel()));
                            break;
                        case CrawlingType.TodayHumor:
                            await WhenAll(crawling.Page, new TodayhumorCrawler(_mongoDbService.Database, source.ToModel()));
                            break;
                        case CrawlingType.FmKorea:
                            await WhenAll(crawling.Page, new FmkoreaCrawler(_mongoDbService.Database, source.ToModel()));
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

        private async Task WhenAll(int page, CrawlerBase crawler)
        {
            await Task.WhenAll(Enumerable.Range(1, page).ToList().ConvertAll(y => crawler.RunAsync(y)).ToArray());
        }
    }
}
