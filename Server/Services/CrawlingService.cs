using EzAspDotNet.Exception;
using EzAspDotNet.Models;
using EzAspDotNet.Notification.Models;
using EzAspDotNet.Services;
using EzAspDotNet.Util;
using EzMongoDb.Util;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebCrawler;
using WebCrawler.Code;
using WebCrawler.Crawler;
using WebCrawler.Models;

namespace Server.Services
{
    public class CrawlingService
    {
        private readonly MongoDbService _mongoDbService;

        private readonly SourceService _sourceService;

        private readonly WebHookService _webHookService;

        private readonly MongoDbUtil<CrawlingData> _mongoCrawlingData;

        public CrawlingService(MongoDbService mongoDbService,
            SourceService sourceService,
            WebHookService webHookService)
        {
            _mongoDbService = mongoDbService;
            _sourceService = sourceService;
            _webHookService = webHookService;
            _mongoCrawlingData = new MongoDbUtil<CrawlingData>(mongoDbService.Database);

            _mongoCrawlingData.Collection.Indexes.CreateMany(new List<CreateIndexModel<CrawlingData>>
            {
                new(Builders<CrawlingData>.IndexKeys.Ascending(x => x.DateTime)),
                new(Builders<CrawlingData>.IndexKeys.Ascending(x => x.Title)),
                new(Builders<CrawlingData>.IndexKeys.Ascending(x => x.Type)),
                new(Builders<CrawlingData>.IndexKeys.Ascending(x => x.Type)
                                                                                   .Ascending(x => x.BoardId)),
                new(Builders<CrawlingData>.IndexKeys.Ascending(x => x.Type)
                                                                                   .Ascending(x => x.BoardId)
                                                                                   .Ascending(x => x.Href),
                                                   new CreateIndexOptions { Unique = true})
            });
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
                ResultCode = EzAspDotNet.Protocols.Code.ResultCode.Success,
                Limit = crawlingList.Limit,
                Offset = crawlingList.Offset,
                Sort = crawlingList.Sort,
                Asc = crawlingList.Asc,
                Datas = MapperUtil.Map<List<CrawlingData>, List<Protocols.Common.CrawlingData>>(await _mongoCrawlingData.Page(filter, crawlingList.Limit, crawlingList.Offset, crawlingList.Sort, crawlingList.Asc)),
                Total = await _mongoCrawlingData.CountAsync(filter)
            };
        }

        public async Task<Protocols.Response.Crawling> Execute(Protocols.Request.Crawling crawling)
        {
            var onCrawlDataDelegate = new CrawlDataDelegate(OnNewCrawlData);
            var sources = crawling.All ?
                          MapperUtil.Map<List<Source>, List<Protocols.Common.Source>>(await _sourceService.All()) :
                          crawling.Sources;

            var crawlerGroup = sources.Select(source =>
            {
                var model = MapperUtil.Map<Source>(source);
                return source.Type switch
                {
                    CrawlingType.DcInside => new DcInsideCrawler(onCrawlDataDelegate, _mongoDbService.Database, model),
                    CrawlingType.Ruliweb => new RuliwebCrawler(onCrawlDataDelegate, _mongoDbService.Database, model),
                    CrawlingType.Clien => new ClienCrawler(onCrawlDataDelegate, _mongoDbService.Database, model),
                    CrawlingType.SlrClub => new SlrclubCrawler(onCrawlDataDelegate, _mongoDbService.Database, model),
                    CrawlingType.Ppomppu => new PpomppuCrawler(onCrawlDataDelegate, _mongoDbService.Database, model),
                    CrawlingType.TodayHumor => new TodayhumorCrawler(onCrawlDataDelegate, _mongoDbService.Database, model),
                    CrawlingType.FmKorea => new FmkoreaCrawler(onCrawlDataDelegate, _mongoDbService.Database, model),
                    CrawlingType.InvenNews => new InvenNewsCrawler(onCrawlDataDelegate, _mongoDbService.Database, model),
                    CrawlingType.HumorUniv => new HumorUnivCrawler(onCrawlDataDelegate, _mongoDbService.Database, model),
                    CrawlingType.Itcm => (CrawlerBase)new ItcmCrawler(onCrawlDataDelegate, _mongoDbService.Database, model),
                    _ => throw new DeveloperException(EzAspDotNet.Protocols.Code.ResultCode.NotImplementedYet),
                };
            }).GroupBy(x => x.GetType());

            foreach(var group in crawlerGroup)
            {
                foreach(var crawler in group)
                {
                    _ = crawler.RunAsync();
                }
            }

            return new Protocols.Response.Crawling
            {
                ResultCode = EzAspDotNet.Protocols.Code.ResultCode.Success
            };
        }

        private async Task OnNewCrawlData(CrawlingData crawlingData)
        {
            var category = string.IsNullOrEmpty(crawlingData.Category) ? string.Empty : $"[{crawlingData.Category}] ";
            await _webHookService.Execute(Builders<Notification>.Filter.Eq(x => x.SourceId, crawlingData.SourceId),
                new EzAspDotNet.Notification.Data.WebHook
                {
                    Title = $"{category}{crawlingData.Title}",
                    Footer = crawlingData.Author,
                    TitleLink = crawlingData.Href,
                    TimeStamp = crawlingData.DateTime.ToTimeStamp()
                });
        }
    }
}
