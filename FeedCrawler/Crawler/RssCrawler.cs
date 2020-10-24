using MongoDB.Driver;
using Serilog;
using System;
using System.Threading.Tasks;
using FeedCrawler.Models;
using WebUtil.Util;
using CodeHollow.FeedReader;
using CodeHollow.FeedReader.Feeds;
using System.Linq;

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

        public virtual async Task<Rss> RunAsync()
        {
            try
            {
                var feed = await FeedReader.ReadAsync(Rss.Url);
                foreach (var item in feed.Items)
                {
                    var link = item.Link.StartsWith("http") ? item.Link : feed.Link + item.Link;
                    if (item.SpecificItem is AtomFeedItem)
                    {
                        var atomFeedItem = item.SpecificItem as AtomFeedItem;
                        link = atomFeedItem.Links.LastOrDefault().Href;
                    }

                    var feedData = new FeedData
                    {
                        Url = Rss.Url,
                        Description = feed.Description,
                        ItemTitle = item.Title,
                        ItemAuthor = item.Author,
                        ItemContent = item.Content,
                        FeedTitle = feed.Title,
                        Href = link,
                        DateTime = feed.LastUpdatedDate.GetValueOrDefault(DateTime.Now)
                    };

                    await OnCrawlData(feedData);
                }

                if (!string.IsNullOrEmpty(Rss.Error))
                {
                    Rss.ErrorTime = null;
                    Rss.Error = string.Empty;
                    return Rss;
                }
            }
            catch (Exception e)
            {
                Rss.Error = e.Message;
                if (!Rss.ErrorTime.HasValue)
                {
                    Rss.ErrorTime = DateTime.Now;
                }
                return Rss;
            }
            return null;
        }

        protected async Task OnCrawlData(FeedData feedData)
        {
            if (MongoDbFeedData == null)
            {
                return;
            }

            await MongoDbFeedData.UpsertAsync(Builders<FeedData>.Filter.Eq(x => x.Url, feedData.Url) &
                    Builders<FeedData>.Filter.Eq(x => x.Href, feedData.Href),
                    feedData,
                    async (feedData) =>
                    {
                        if (OnCrawlDataDelegate != null)
                        {
                            await OnCrawlDataDelegate.Invoke(feedData);
                        }
                    });
        }
    }
}
