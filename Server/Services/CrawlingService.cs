using WebUtil.Util;
using System.Linq;
using System.Threading.Tasks;
using WebUtil.Services;
using WebCrawler;
using MongoDB.Driver;
using WebCrawler.Code;

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
                            await Task.WhenAll(Enumerable.Range(1, crawling.Page).ToList().ConvertAll(y => new RuliwebCrawler(_mongoDbService.Database, source.BoardId.ToInt(), y).RunAsync()).ToArray());
                            break;
                        case CrawlingType.Clien:
                            await Task.WhenAll(Enumerable.Range(1, crawling.Page).ToList().ConvertAll(y => new ClienCrawler(_mongoDbService.Database, source.BoardId, y).RunAsync()).ToArray());
                            break;
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
