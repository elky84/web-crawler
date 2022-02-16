using System.Threading.Tasks;
using EzAspDotNet.Services;
using MongoDB.Driver;
using System.Collections.Generic;
using EzAspDotNet.Exception;
using FeedCrawler.Models;
using Server.Models;
using EzAspDotNet.Util;

namespace Server.Services
{
    public class RssService
    {
        private readonly MongoDbUtil<Rss> _mongoDbRss;

        public RssService(MongoDbService mongoDbService)
        {
            _mongoDbRss = new MongoDbUtil<Rss>(mongoDbService.Database);

            _mongoDbRss.Collection.Indexes.CreateOne(new CreateIndexModel<Rss>(
                Builders<Rss>.IndexKeys.Ascending(x => x.Url)));
        }

        public async Task<List<Rss>> All()
        {
            return await _mongoDbRss.All();
        }

        public async Task<List<Rss>> Error()
        {
            return await _mongoDbRss.FindAsync(Builders<Rss>.Filter.Ne(x => x.ErrorTime, null));
        }

        public async Task<Protocols.Response.Rss> Create(Protocols.Request.Rss rss)
        {
            var created = await Create(rss.Data);

            return new Protocols.Response.Rss
            {
                ResultCode = Code.ResultCode.Success,
                Data = created?.ToProtocol()
            };

        }

        private async Task<Rss> Create(Protocols.Common.Rss rss)
        {
            try
            {
                return await _mongoDbRss.UpsertAsync(Builders<Rss>.Filter.Eq(x => x.Url, rss.Url), rss.ToModel());
            }
            catch (MongoWriteException)
            {
                throw new DeveloperException(Code.ResultCode.UsingRssId);
            }
        }

        public async Task<Protocols.Response.RssMulti> CreateMulti(Protocols.Request.RssMulti rssMulti)
        {
            var rsss = new List<Rss>();
            foreach (var rss in rssMulti.Datas)
            {
                rsss.Add(await Create(rss));
            }

            return new Protocols.Response.RssMulti
            {
                Datas = rsss.ConvertAll(x => x.ToProtocol())
            };
        }

        public async Task<Protocols.Response.Rss> GetById(string id)
        {
            return new Protocols.Response.Rss
            {
                ResultCode = Code.ResultCode.Success,
                Data = (await _mongoDbRss.FindOneAsyncById(id))?.ToProtocol()
            };
        }

        public async Task<Rss> Get(string url)
        {
            return await _mongoDbRss.FindOneAsync(Builders<Rss>.Filter.Eq(x => x.Url, url));
        }

        public async Task<Protocols.Response.Rss> Update(string id, Protocols.Request.Rss rss)
        {
            var update = rss.Data.ToModel();

            var updated = await _mongoDbRss.UpdateAsync(id, update);
            return new Protocols.Response.Rss
            {
                ResultCode = Code.ResultCode.Success,
                Data = (updated ?? update).ToProtocol()
            };
        }


        public async Task<Rss> Update(Rss rss)
        {
            return await _mongoDbRss.UpdateAsync(rss.Id, rss);
        }

        public async Task<Protocols.Response.Rss> Delete(string id)
        {
            return new Protocols.Response.Rss
            {
                ResultCode = Code.ResultCode.Success,
                Data = (await _mongoDbRss.RemoveGetAsync(id))?.ToProtocol()
            };
        }
    }
}
