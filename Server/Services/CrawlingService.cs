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
                            await new RuliwebCrawler(_mongoDbService.Database, source.ToModel()).RunAsync();
                            break;
                        case CrawlingType.Clien:
                            await new ClienCrawler(_mongoDbService.Database, source.ToModel()).RunAsync();
                            break;
                        case CrawlingType.SlrClub:
                            await new SlrclubCrawler(_mongoDbService.Database, source.ToModel()).RunAsync();
                            break;
                        case CrawlingType.Ppomppu:
                            await new PpomppuCrawler(_mongoDbService.Database, source.ToModel()).RunAsync();
                            break;
                        case CrawlingType.TodayHumor:
                            await new TodayhumorCrawler(_mongoDbService.Database, source.ToModel()).RunAsync();
                            break;
                        case CrawlingType.FmKorea:
                            await new FmkoreaCrawler(_mongoDbService.Database, source.ToModel()).RunAsync();
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
    }
}
