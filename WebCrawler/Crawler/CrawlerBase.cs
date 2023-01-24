using Abot2.Core;
using Abot2.Crawler;
using Abot2.Poco;
using EzAspDotNet.Util;
using EzMongoDb.Util;
using MongoDB.Driver;
using Serilog;
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using WebCrawler.Models;

namespace WebCrawler
{
    public delegate Task CrawlDataDelegate(CrawlingData data);

    public abstract class CrawlerBase
    {
        protected string UrlBase { get; set; }

        protected PoliteWebCrawler CrawlerInstance { get; set; }

        protected Source Source { get; set; }

        protected MongoDbUtil<CrawlingData> MongoDbCrawlingData;

        public CrawlDataDelegate OnCrawlDataDelegate { get; set; }

        protected int Executing;

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

        protected virtual CrawlConfiguration Config()
        {
            return new CrawlConfiguration
            {
                MaxConcurrentThreads = 10,
                MaxPagesToCrawl = 1,
                MaxPagesToCrawlPerDomain = 10,
                MinRetryDelayInMilliseconds = 1000,
                MinCrawlDelayPerDomainMilliSeconds = 1000,
                IsSendingCookiesEnabled = true,
                UserAgentString = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/109.0.0.0 Safari/537.36",
                RobotsDotTextUserAgentString = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/109.0.0.0 Safari/537.36",
            };
        }

        public PoliteWebCrawler Create()
        {
            if (CrawlerInstance != null)
            {
                return CrawlerInstance;
            }

            CrawlerInstance = new PoliteWebCrawler(Config(), null, null, null, new PageRequester(Config(), new WebContentExtractor()), null, null, null, null);
            CrawlerInstance.PageCrawlStarting += ProcessPageCrawlStarting;
            CrawlerInstance.PageCrawlCompleted += ProcessPageCrawlCompleted;
            CrawlerInstance.PageCrawlDisallowed += PageCrawlDisallowed;
            CrawlerInstance.PageLinksCrawlDisallowed += PageLinksCrawlDisallowed;
            return CrawlerInstance;
        }

        public virtual async Task RunAsync()
        {
            Create();

            for (var page = Source.PageMin; page <= Source.PageMax; ++page)
            {
                await ExecuteAsync(page);
                Thread.Sleep(Source.Interval);
            }
        }

        protected virtual bool CanTwice() => true;

        protected async Task<bool> ExecuteAsync(int page)
        {
            if (CanTwice() || 0 == Interlocked.Exchange(ref Executing, 1))
            {
                var builder = new UriBuilder(UrlComposite(page));
                var crawlResult = await CrawlerInstance.CrawlAsync(builder.Uri);
                Interlocked.Exchange(ref Executing, 0);
                return crawlResult.ErrorOccurred;
            }

            return false;
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
            if (crawledPage.HttpRequestException != null ||
                crawledPage.HttpResponseMessage.StatusCode != HttpStatusCode.OK ||
                string.IsNullOrEmpty(crawledPage.Content.Text))
            {
                Log.Logger.Error($"Crawl of page failed. <Url:{crawledPage.Uri.AbsoluteUri}> " +
                    $"<StatusCode:{crawledPage.HttpResponseMessage.StatusCode}> " +
                    $"<Exception:{crawledPage.HttpRequestException}>" +
                    $"<Content:{crawledPage.HttpResponseMessage.Content}>");
                return;
            }
            else
            {
                Log.Logger.Debug($"Crawl of page succeeded {crawledPage.Uri.AbsoluteUri}");
            }

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

        protected async Task OnCrawlData(CrawlingData crawlingData)
        {
            if (MongoDbCrawlingData == null)
            {
                return;
            }

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

            await MongoDbCrawlingData.UpsertAsync(filter, crawlingData,
                async (crawlingData) =>
                {
                    if (OnCrawlDataDelegate != null)
                    {
                        await OnCrawlDataDelegate.Invoke(crawlingData);
                    }
                });
        }
    }
}
