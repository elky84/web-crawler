﻿using Abot2.Core;
using Abot2.Crawler;
using Abot2.Poco;
using AngleSharp;
using MongoDB.Driver;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WebCrawler.Models;
using WebUtil.Util;

namespace WebCrawler
{
    public abstract class CrawlerBase
    {
        protected string UrlBase { get; set; }

        protected PoliteWebCrawler CrawlerInstance { get; set; }

        protected Source Source { get; set; }

        protected MongoDbUtil<CrawlingData> MongoDbCrawlingData;

        protected int PagePerInterval { get; set; }

        protected IMongoDatabase MongoDb { get; set; }

        public delegate Task CrawlDataDelegate(CrawlingData data);

        public CrawlDataDelegate OnCrawlDataDelegate { get; set; }

        public CrawlerBase(CrawlDataDelegate onCrawlDataDelegate, IMongoDatabase mongoDb, string urlBase, Source source)
        {
            if (mongoDb != null)
            {
                MongoDbCrawlingData = new MongoDbUtil<CrawlingData>(mongoDb);
            }

            OnCrawlDataDelegate = onCrawlDataDelegate;
            UrlBase = urlBase;
            Source = source;
        }

        public PoliteWebCrawler Create()
        {
            var config = new CrawlConfiguration
            {
                MaxConcurrentThreads = 10,
                MaxPagesToCrawl = 1,
                MaxPagesToCrawlPerDomain = 10,
                MinRetryDelayInMilliseconds = 1000,
                MinCrawlDelayPerDomainMilliSeconds = 1000,
                IsSendingCookiesEnabled = true
            };

            CrawlerInstance = new PoliteWebCrawler(config, null, null, null, new PageRequester(config, new WebContentExtractor()), null, null, null, null);
            CrawlerInstance.PageCrawlStarting += ProcessPageCrawlStarting;
            CrawlerInstance.PageCrawlCompleted += ProcessPageCrawlCompleted;
            CrawlerInstance.PageCrawlDisallowed += PageCrawlDisallowed;
            CrawlerInstance.PageLinksCrawlDisallowed += PageLinksCrawlDisallowed;
            return CrawlerInstance;
        }

        public virtual async Task RunAsync()
        {
            if (CrawlerInstance == null)
            {
                Create();
            }

            for (var page = Source.PageMin; page <= Source.PageMax; ++page)
            {
                await ExecuteAsync(page);
                Thread.Sleep(Source.Interval);
            }
        }

        protected async Task<bool> ExecuteAsync(int page)
        {
            var builder = new UriBuilder(UrlComposite(page));

            var crawlResult = await CrawlerInstance.CrawlAsync(builder.Uri);

            return crawlResult.ErrorOccurred;
        }

        protected abstract string UrlComposite(int page);

        void ProcessPageCrawlStarting(object sender, PageCrawlStartingArgs e)
        {
            PageToCrawl pageToCrawl = e.PageToCrawl;
            Log.Logger.Debug($"About to crawl link {pageToCrawl.Uri.AbsoluteUri} which was found on page {pageToCrawl.ParentUri.AbsoluteUri}");
        }

        void ProcessPageCrawlCompleted(object sender, PageCrawlCompletedArgs e)
        {
            CrawledPage crawledPage = e.CrawledPage;
            if (crawledPage.HttpRequestException != null || crawledPage.HttpResponseMessage.StatusCode != HttpStatusCode.OK)
                Log.Logger.Error($"Crawl of page failed {crawledPage.Uri.AbsoluteUri}");
            else
                Log.Logger.Information($"Crawl of page succeeded {crawledPage.Uri.AbsoluteUri}");

            if (string.IsNullOrEmpty(crawledPage.Content.Text))
                Log.Logger.Debug($"Page had no content {crawledPage.Uri.AbsoluteUri}");

            OnPageCrawl(crawledPage.AngleSharpHtmlDocument);
        }

        protected abstract void OnPageCrawl(AngleSharp.Html.Dom.IHtmlDocument document);

        protected virtual string UrlCompositeHref(string href)
        {
            return UrlBase.CutAndComposite("/", 0, 3, href);
        }

        void PageLinksCrawlDisallowed(object sender, PageLinksCrawlDisallowedArgs e)
        {
            CrawledPage crawledPage = e.CrawledPage;
            Log.Logger.Error($"Did not crawl the links on page {crawledPage.Uri.AbsoluteUri} due to {e.DisallowedReason}");
        }

        void PageCrawlDisallowed(object sender, PageCrawlDisallowedArgs e)
        {
            PageToCrawl pageToCrawl = e.PageToCrawl;
            Log.Logger.Error($"Did not crawl page {pageToCrawl.Uri.AbsoluteUri} due to {e.DisallowedReason}");
        }

        private async Task<CrawlingData> GetOriginData(CrawlingData crawlingData)
        {
            var builder = Builders<CrawlingData>.Filter;
            var filter = builder.Eq(x => x.Type, crawlingData.Type);
            if (crawlingData.RowId.HasValue)
            {
                filter &= builder.Eq(x => x.RowId, crawlingData.RowId.Value);
            }
            else
            {
                filter &= Builders<CrawlingData>.Filter.Eq(x => x.Title, crawlingData.Title);
            }

            return await MongoDbCrawlingData.FindOneAsync(filter);
        }

        protected async Task OnCrawlData(CrawlingData crawlingData)
        {
            if (MongoDbCrawlingData == null)
            {
                return;
            }

            var origin = await GetOriginData(crawlingData);
            if (origin != null)
            {
                crawlingData.Id = origin.Id;
                await MongoDbCrawlingData.UpdateAsync(crawlingData.Id, crawlingData);
            }
            else
            {
                await MongoDbCrawlingData.CreateAsync(crawlingData);
                await OnCrawlDataDelegate?.Invoke(crawlingData);
            }
        }
    }
}
