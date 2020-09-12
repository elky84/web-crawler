using WebUtil.Util;
using System.Linq;
using System.Threading.Tasks;
using WebUtil.Services;
using WebCrawler;
using MongoDB.Driver;
using WebCrawler.Code;
using Server.Models;
using System.Collections.Generic;
using Server.Exception;

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
            try
            {
                var created = await _mongoDbSource.CreateAsync(new Source
                {
                    Type = source.Type,
                    BoardId = source.BoardId
                });

                return new Protocols.Response.Source
                {
                    ResultCode = Code.ResultCode.Success,
                    SourceData = created?.ToProtocol()
                };
            }
            catch (MongoWriteException)
            {
                throw new DeveloperException(Code.ResultCode.UsingSourceId);
            }
        }

        public async Task<Protocols.Response.Source> Get(string id)
        {
            return new Protocols.Response.Source
            {
                ResultCode = Code.ResultCode.Success,
                SourceData = (await _mongoDbSource.FindOneAsyncById(id))?.ToProtocol()
            };
        }

        public async Task<Protocols.Response.Source> Update(string id, Protocols.Request.Source source)
        {
            var update = new Source
            {
                Id = id,
                Type = source.Type,
                BoardId = source.BoardId
            };

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
