using MongoDB.Driver;
using Serilog;
using System;
using System.Threading.Tasks;
using FeedCrawler.Models;
using WebUtil.Util;
using CodeHollow.FeedReader;

namespace FeedCrawler.Crawler
{
    public class RssCrawler
    {
        protected Rss Rss { get; set; }

        protected MongoDbUtil<FeedData> MongoDbFeedData;


        public delegate Task CrawlDataDelegate(FeedData data);

        public CrawlDataDelegate OnCrawlDataDelegate { get; set; }

        public RssCrawler(CrawlDataDelegate onCrawlDataDelegate, IMongoDatabase mongoDb, Rss rss)
        {
            if (mongoDb != null)
            {
                MongoDbFeedData = new MongoDbUtil<FeedData>(mongoDb);
            }

            OnCrawlDataDelegate = onCrawlDataDelegate;
            Rss = rss;
        }

        public virtual async Task RunAsync()
        {
            try
            {
                var feed = await FeedReader.ReadAsync(Rss.Url);

                foreach (var item in feed.Items)
                {
                    var feedData = new FeedData
                    {
                        Url = Rss.Url,
                        Description = feed.Description,
                        ItemTitle = item.Title,
                        ItemAuthor = item.Author,
                        ItemContent = item.Content,
                        FeedTitle = feed.Title,
                        Href = item.Link.StartsWith("http") ? item.Link : feed.Link + item.Link,
                        DateTime = feed.LastUpdatedDate.GetValueOrDefault(DateTime.Now)
                    };

                    await OnCrawlData(feedData);
                }
            }
            catch (Exception e)
            {
                Log.Error(e.Message);
            }

        }

        private async Task<FeedData> GetOriginData(FeedData feedData)
        {
            return await MongoDbFeedData.FindOneAsync(Builders<FeedData>.Filter.Eq(x => x.Url, feedData.Url) &
                    Builders<FeedData>.Filter.Eq(x => x.ItemTitle, feedData.ItemTitle));
        }

        protected async Task OnCrawlData(FeedData feedData)
        {
            if (MongoDbFeedData == null)
            {
                return;
            }

            var origin = await GetOriginData(feedData);
            if (origin != null)
            {
                feedData.Id = origin.Id;
                await MongoDbFeedData.UpdateAsync(feedData.Id, feedData);
            }
            else
            {
                await MongoDbFeedData.CreateAsync(feedData);

                if (OnCrawlDataDelegate != null)
                {
                    await OnCrawlDataDelegate.Invoke(feedData);
                }
            }
        }
    }
}
