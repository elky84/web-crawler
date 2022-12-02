using EzAspDotNet.Exception;
using EzAspDotNet.Models;
using EzAspDotNet.Services;
using EzMongoDb.Util;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;
using WebCrawler.Code;
using WebCrawler.Models;

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
            return await _mongoDbSource.FindAsync(Builders<Source>.Filter.Eq(x => x.Switch, true));
        }

        public async Task<Protocols.Response.Source> Create(Protocols.Request.Source source)
        {
            var created = await Create(source.Data);
            if (created == null)
            {
                throw new DeveloperException(Code.ResultCode.CreateFailedSource);
            }

            return new Protocols.Response.Source
            {
                ResultCode = EzAspDotNet.Protocols.Code.ResultCode.Success,
                Data = MapperUtil.Map<Protocols.Common.Source>(created),
            };

        }

        private async Task<Source> Create(Protocols.Common.Source source)
        {
            try
            {
                return await _mongoDbSource.UpsertAsync(Builders<Source>.Filter.Eq(x => x.Type, source.Type) &
                                                        Builders<Source>.Filter.Eq(x => x.BoardId, source.BoardId),
                                                        MapperUtil.Map<Source>(source));
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
                Datas = MapperUtil.Map<List<Source>,
                                       List<Protocols.Common.Source>>
                                       (sources)
            };
        }

        public async Task<Protocols.Response.Source> GetById(string id)
        {
            var source = await _mongoDbSource.FindOneAsyncById(id);
            if (source == null)
            {
                throw new DeveloperException(Code.ResultCode.CreateFailedSource);
            }

            return new Protocols.Response.Source
            {
                ResultCode = Code.ResultCode.Success,
                Data = MapperUtil.Map<Protocols.Common.Source>(source)
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
            var update = MapperUtil.Map<Source>(source.Data);
            update.Created = source.Data.Created;

            var updated = await _mongoDbSource.UpdateAsync(id, update);
            if (updated == null)
            {
                throw new DeveloperException(Code.ResultCode.NotFoundSource);
            }

            return new Protocols.Response.Source
            {
                ResultCode = EzAspDotNet.Protocols.Code.ResultCode.Success,
                Data = MapperUtil.Map<Protocols.Common.Source>(updated)
            };
        }

        public async Task<Protocols.Response.Source> Delete(string id)
        {
            var deleted = await _mongoDbSource.RemoveGetAsync(id);
            if (deleted == null)
            {
                throw new DeveloperException(Code.ResultCode.NotFoundSource);
            }

            return new Protocols.Response.Source
            {
                ResultCode = Code.ResultCode.Success,
                Data = MapperUtil.Map<Protocols.Common.Source>(deleted)
            };
        }
    }
}
