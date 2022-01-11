using System.Threading.Tasks;
using MongoDbWebUtil.Services;
using MongoDB.Driver;
using WebCrawler.Models;
using System.Collections.Generic;
using WebCrawler.Code;
using Server.Models;
using EzAspDotNet.Exception;
using MongoDbWebUtil.Util;

namespace Server.Services
{
    public class SourceService
    {
        private readonly MongoDbUtil<Source> _mongoDbSource;

        public SourceService(MongoDbService mongoDbService)
        {
            _mongoDbSource = new MongoDbUtil<Source>(mongoDbService.Database);

            _mongoDbSource.Collection.Indexes.CreateOne(new CreateIndexModel<Source>(
                Builders<Source>.IndexKeys.Ascending(x => x.Type)));

            _mongoDbSource.Collection.Indexes.CreateOne(new CreateIndexModel<Source>(
                Builders<Source>.IndexKeys.Ascending(x => x.Type).Ascending(x => x.BoardId)));
        }

        public async Task<List<Source>> All()
        {
            return await _mongoDbSource.All();
        }

        public async Task<Protocols.Response.Source> Create(Protocols.Request.Source source)
        {
            var created = await Create(source.Data);

            return new Protocols.Response.Source
            {
                ResultCode = Code.ResultCode.Success,
                Data = created?.ToProtocol()
            };

        }

        private async Task<Source> Create(Protocols.Common.Source source)
        {
            try
            {
                return await _mongoDbSource.UpsertAsync(Builders<Source>.Filter.Eq(x => x.Type, source.Type) & Builders<Source>.Filter.Eq(x => x.BoardId, source.BoardId), source.ToModel());
            }
            catch (MongoWriteException)
            {
                throw new DeveloperException(Code.ResultCode.UsingSourceId);
            }
        }

        public async Task<Protocols.Response.SourceMulti> CreateMulti(Protocols.Request.SourceMulti sourceMulti)
        {
            var sources = new List<Source>();
            foreach (var source in sourceMulti.Datas)
            {
                sources.Add(await Create(source));
            }

            return new Protocols.Response.SourceMulti
            {
                Datas = sources.ConvertAll(x => x.ToProtocol())
            };
        }

        public async Task<Protocols.Response.Source> GetById(string id)
        {
            return new Protocols.Response.Source
            {
                ResultCode = Code.ResultCode.Success,
                Data = (await _mongoDbSource.FindOneAsyncById(id))?.ToProtocol()
            };
        }

        public async Task<Source> Get(CrawlingType crawlingType, string boardId)
        {
            return await _mongoDbSource.FindOneAsync(Builders<Source>.Filter.Eq(x => x.Type, crawlingType) &
                Builders<Source>.Filter.Eq(x => x.BoardId, boardId));
        }

        public async Task<Source> GetByName(CrawlingType crawlingType, string boardName)
        {
            return await _mongoDbSource.FindOneAsync(Builders<Source>.Filter.Eq(x => x.Type, crawlingType) &
                Builders<Source>.Filter.Eq(x => x.Name, boardName));
        }

        public async Task<Protocols.Response.Source> Update(string id, Protocols.Request.Source source)
        {
            var update = source.Data.ToModel();

            var updated = await _mongoDbSource.UpdateAsync(id, update);
            return new Protocols.Response.Source
            {
                ResultCode = Code.ResultCode.Success,
                Data = (updated ?? update).ToProtocol()
            };
        }

        public async Task<Protocols.Response.Source> Delete(string id)
        {
            return new Protocols.Response.Source
            {
                ResultCode = Code.ResultCode.Success,
                Data = (await _mongoDbSource.RemoveGetAsync(id))?.ToProtocol()
            };
        }
    }
}
