﻿using WebUtil.Util;
using System.Threading.Tasks;
using WebUtil.Services;
using MongoDB.Driver;
using WebCrawler.Models;
using Server.Models;
using System.Collections.Generic;
using Server.Exception;
using WebCrawler.Code;

namespace Server.Services
{
    public class SourceService
    {
        private readonly MongoDbUtil<Source> _mongoDbSource;

        public SourceService(MongoDbService mongoDbService)
        {
            _mongoDbSource = new MongoDbUtil<Source>(mongoDbService.Database);
        }

        public async Task<List<Source>> All()
        {
            return await _mongoDbSource.All();
        }

        public async Task<Protocols.Response.Source> Create(Protocols.Request.Source source)
        {
            var created = await Create(source.SourceData);

            return new Protocols.Response.Source
            {
                ResultCode = Code.ResultCode.Success,
                SourceData = created?.ToProtocol()
            };

        }

        private async Task<Source> Create(Protocols.Common.Source source)
        {
            try
            {
                var newData = source.ToModel();

                var origin = await _mongoDbSource.FindOneAsync(Builders<Source>.Filter.Eq(x => x.Type, source.Type) & Builders<Source>.Filter.Eq(x => x.BoardId, source.BoardId));
                if (origin != null)
                {
                    newData.Id = origin.Id;
                    await _mongoDbSource.UpdateAsync(newData.Id, newData);
                    return newData;
                }
                else
                {
                    return await _mongoDbSource.CreateAsync(source.ToModel());
                }
            }
            catch (MongoWriteException)
            {
                throw new DeveloperException(Code.ResultCode.UsingSourceId);
            }
        }

        public async Task<Protocols.Response.SourceMulti> CreateMulti(Protocols.Request.SourceMulti sourceMulti)
        {
            var sources = new List<Source>();
            foreach (var source in sourceMulti.SourceDatas)
            {
                sources.Add(await Create(source));
            }

            return new Protocols.Response.SourceMulti
            {
                SourceDatas = sources.ConvertAll(x => x.ToProtocol())
            };
        }

        public async Task<Protocols.Response.Source> GetById(string id)
        {
            return new Protocols.Response.Source
            {
                ResultCode = Code.ResultCode.Success,
                SourceData = (await _mongoDbSource.FindOneAsyncById(id))?.ToProtocol()
            };
        }

        public async Task<Source> Get(CrawlingType crawlingType, string boardId)
        {
            return await _mongoDbSource.FindOneAsync(Builders<Source>.Filter.Eq(x => x.Type, crawlingType) &
                Builders<Source>.Filter.Eq(x => x.BoardId, boardId));
        }

        public async Task<Source> Get(string boardName)
        {
            return await _mongoDbSource.FindOneAsync(Builders<Source>.Filter.Eq(x => x.Name, boardName));
        }

        public async Task<Protocols.Response.Source> Update(string id, Protocols.Request.Source source)
        {
            var update = source.SourceData.ToModel();

            var updated = await _mongoDbSource.UpdateAsync(id, update);
            return new Protocols.Response.Source
            {
                ResultCode = Code.ResultCode.Success,
                SourceData = (updated ?? update).ToProtocol()
            };
        }

        public async Task<Protocols.Response.Source> Delete(string id)
        {
            return new Protocols.Response.Source
            {
                ResultCode = Code.ResultCode.Success,
                SourceData = (await _mongoDbSource.RemoveAsync(id))?.ToProtocol()
            };
        }
    }
}
