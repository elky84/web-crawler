using Abot2.Core;
using Abot2.Crawler;
using Abot2.Poco;
using AngleSharp.Html.Parser;
using EzAspDotNet.Util;
using EzMongoDb.Util;
using MongoDB.Driver;
using Serilog;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AngleSharp;
using AngleSharp.Dom;
using WebCrawler.Models;

namespace WebCrawler.Crawler
{
    public delegate Task CrawlDataDelegate(CrawlingData data);

    public abstract class CrawlerBase
    {
        protected string UrlBase { get; set; }

        protected Source Source { get; set; }

        private readonly MongoDbUtil<CrawlingData> _mongoDbCrawlingData;

        private CrawlDataDelegate OnCrawlDataDelegate { get; set; }

        private HtmlParser _parser = new();

        private int _executing;

        protected ConcurrentBag<CrawlingData> ConcurrentBag { get; } = [];

        protected CrawlerBase(CrawlDataDelegate onCrawlDataDelegate, IMongoDatabase mongoDb, string urlBase, Source source)
        {
            if (mongoDb != null)
            {
                _mongoDbCrawlingData = new MongoDbUtil<CrawlingData>(mongoDb);
            }

            OnCrawlDataDelegate = onCrawlDataDelegate;
            UrlBase = urlBase;
            Source = source;
        }

        protected virtual CrawlConfiguration Config()
        {
            return new CrawlConfiguration
            {
                MaxConcurrentThreads = 10,
                MaxPagesToCrawl = 1,
                MaxPagesToCrawlPerDomain = 5,
                MinRetryDelayInMilliseconds = 1000,
                MinCrawlDelayPerDomainMilliSeconds = 5000,
                IsForcedLinkParsingEnabled = true,
                HttpProtocolVersion = HttpProtocolVersion.Version11,
                UserAgentString = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36"
            };
        }

        protected virtual PoliteWebCrawler Create()
        {
            var crawlerInstance = new PoliteWebCrawler(Config(), 
                null, 
                null, 
                null,
                new PageRequester(Config(), new WebContentExtractor(), CreateHttpClient()), 
                null, 
                null, 
                null, 
                null);
            crawlerInstance.PageCrawlStarting += ProcessPageCrawlStarting;
            crawlerInstance.PageCrawlCompleted += ProcessPageCrawlCompleted;
            crawlerInstance.PageCrawlDisallowed += PageCrawlDisallowed;
            crawlerInstance.PageLinksCrawlDisallowed += PageLinksCrawlDisallowed;
            return crawlerInstance;
        }
        
        
        private static HttpClient CreateHttpClient()
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Accept-Charset", "utf-8");
            return client;
        }

        public virtual async Task RunAsync()
        {
            var crawlerInstance = Create();

            for (var page = Source.PageMin; page <= Source.PageMax; ++page)
            {
                await ExecuteAsync(crawlerInstance, page);
                Thread.Sleep(Source.Interval);
            }
        }

        protected virtual bool CanTwice() => true;

        protected async Task<bool> ExecuteAsync(PoliteWebCrawler crawler, int page)
        {
            if (!CanTwice() && 0 != Interlocked.Exchange(ref _executing, 1)) return false;
            var builder = new UriBuilder(UrlComposite(page));
            var crawlResult = await crawler.CrawlAsync(builder.Uri);
            Interlocked.Exchange(ref _executing, 0);
            return crawlResult.ErrorOccurred;

        }

        protected abstract string UrlComposite(int page);

        private void ProcessPageCrawlStarting(object sender, PageCrawlStartingArgs e)
        {
            ConcurrentBag.Clear();
            var pageToCrawl = e.PageToCrawl;
            Log.Logger.Debug("About to crawl link {UriAbsoluteUri} which was found on page {ParentUriAbsoluteUri}", pageToCrawl.Uri.AbsoluteUri, pageToCrawl.ParentUri.AbsoluteUri);
        }

        private void ProcessPageCrawlCompleted(object sender, PageCrawlCompletedArgs e)
        {
            var crawledPage = e.CrawledPage;
            if (crawledPage.HttpRequestException != null)
            {
                Log.Logger.Error("Crawl of page failed. <Url:{UriAbsoluteUri}> <Exception:{Message}>", crawledPage.Uri?.AbsoluteUri, crawledPage.HttpRequestException?.Message);
                return;
            }

            if (crawledPage.HttpResponseMessage?.StatusCode != HttpStatusCode.OK)
            {
                Log.Logger.Error("Crawl of page failed. <Url:{UriAbsoluteUri}> <StatusCode:{StatusCode}>", crawledPage.Uri?.AbsoluteUri, crawledPage.HttpResponseMessage?.StatusCode);
                return;
            }

            if (string.IsNullOrEmpty(crawledPage.Content?.Text))
            {
                Log.Logger.Error("Crawl of page failed. <Url:{UriAbsoluteUri}> <Content:{Content}>", crawledPage.Uri?.AbsoluteUri, crawledPage.HttpResponseMessage?.Content);
                return;
            }

            Log.Logger.Debug("Crawl of page succeeded {UriAbsoluteUri}", crawledPage.Uri?.AbsoluteUri);

            OnPageCrawl(crawledPage.AngleSharpHtmlDocument);
        }

        protected abstract void OnPageCrawl(AngleSharp.Html.Dom.IHtmlDocument document);

        protected virtual string UrlCompositeHref(string href)
        {
            return UrlBase.CutAndComposite("/", 0, 3, href);
        }

        private static void PageLinksCrawlDisallowed(object sender, PageLinksCrawlDisallowedArgs e)
        {
            var crawledPage = e.CrawledPage;
            Log.Logger.Error("Did not crawl the links on page {UriAbsoluteUri} due to {EDisallowedReason}", crawledPage.Uri.AbsoluteUri, e.DisallowedReason);
        }

        private static void PageCrawlDisallowed(object sender, PageCrawlDisallowedArgs e)
        {
            var pageToCrawl = e.PageToCrawl;
            Log.Logger.Error("Did not crawl page {UriAbsoluteUri} due to {EDisallowedReason}", pageToCrawl.Uri.AbsoluteUri, e.DisallowedReason);
        }

        protected async Task<CrawlingData> OnCrawlData(CrawlingData crawlingData)
        {
            // 글자 뒤의 공백 날리기
            crawlingData.Title = crawlingData.Title.TrimEnd();

            // 현재시간보다 크다면, 시간만 담긴 데이터에서 전날 글에 대한 시간 + 오늘 날짜로 값이 들어와서 그런 것. 이에 대한 예외처리
            if (crawlingData.DateTime > DateTime.Now)
            {
                crawlingData.DateTime = crawlingData.DateTime.AddDays(-1);
            }

            var builder = Builders<CrawlingData>.Filter;
            var filter = builder.Eq(x => x.Type, crawlingData.Type) &
                builder.Eq(x => x.BoardId, crawlingData.BoardId) &
                builder.Eq(x => x.Href, crawlingData.Href);

            if (_mongoDbCrawlingData != null)
            {
                await _mongoDbCrawlingData.UpsertAsync(filter, crawlingData,
                    CreateAction);
            }

            return crawlingData;
        }

        private async void CreateAction(CrawlingData crawlingData)
        {
            if (OnCrawlDataDelegate != null)
            {
                await OnCrawlDataDelegate.Invoke(crawlingData);
            }
        }
    }
}
