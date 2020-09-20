﻿using WebUtil.Util;
using System.Threading.Tasks;
using WebUtil.Services;
using MongoDB.Driver;
using Server.Models;
using System.Collections.Generic;
using Server.Exception;
using WebCrawler.Code;
using FeedCrawler.Models;

namespace Server.Services
{
    public class RssService
    {
        private readonly MongoDbUtil<Rss> _mongoDbRss;

        public RssService(MongoDbService mongoDbService)
        {
            _mongoDbRss = new MongoDbUtil<Rss>(mongoDbService.Database);
        }

        public async Task<List<Rss>> All()
        {
            return await _mongoDbRss.All();
        }

        public async Task<Protocols.Response.Rss> Create(Protocols.Request.Rss rss)
        {
            var created = await Create(rss.RssData);

            return new Protocols.Response.Rss
            {
                ResultCode = Code.ResultCode.Success,
                RssData = created?.ToProtocol()
            };

        }

        private async Task<Rss> Create(Protocols.Common.Rss rss)
        {
            try
            {
                var newData = rss.ToModel();

                var origin = await _mongoDbRss.FindOneAsync(Builders<Rss>.Filter.Eq(x => x.Url, rss.Url));
                if (origin != null)
                {
                    newData.Id = origin.Id;
                    await _mongoDbRss.UpdateAsync(newData.Id, newData);
                    return newData;
                }
                else
                {
                    return await _mongoDbRss.CreateAsync(rss.ToModel());
                }
            }
            catch (MongoWriteException)
            {
                throw new DeveloperException(Code.ResultCode.UsingRssId);
            }
        }

        public async Task<Protocols.Response.RssMulti> CreateMulti(Protocols.Request.RssMulti rssMulti)
        {
            var rsss = new List<Rss>();
            foreach (var rss in rssMulti.RssDatas)
            {
                rsss.Add(await Create(rss));
            }

            return new Protocols.Response.RssMulti
            {
                RssDatas = rsss.ConvertAll(x => x.ToProtocol())
            };
        }

        public async Task<Protocols.Response.Rss> GetById(string id)
        {
            return new Protocols.Response.Rss
            {
                ResultCode = Code.ResultCode.Success,
                RssData = (await _mongoDbRss.FindOneAsyncById(id))?.ToProtocol()
            };
        }

        public async Task<Rss> Get(string url)
        {
            return await _mongoDbRss.FindOneAsync(Builders<Rss>.Filter.Eq(x => x.Url, url));
        }

        public async Task<Protocols.Response.Rss> Update(string id, Protocols.Request.Rss rss)
        {
            var update = rss.RssData.ToModel();

            var updated = await _mongoDbRss.UpdateAsync(id, update);
            return new Protocols.Response.Rss
            {
                ResultCode = Code.ResultCode.Success,
                RssData = (updated ?? update).ToProtocol()
            };
        }

        public async Task<Protocols.Response.Rss> Delete(string id)
        {
            return new Protocols.Response.Rss
            {
                ResultCode = Code.ResultCode.Success,
                RssData = (await _mongoDbRss.RemoveAsync(id))?.ToProtocol()
            };
        }
    }
}
