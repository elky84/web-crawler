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
using System.Net.Http.Headers;
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
        
        private int _executing;
        
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
        
        private static HttpClient CreateHttpClient()
        {
            var handler = new HttpClientHandler { UseCookies = true};

            var client = new HttpClient(handler);
            client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/130.0.0.0 Safari/537.36 Edg/130.0.0.0");
            client.DefaultRequestHeaders.CacheControl = new CacheControlHeaderValue { NoCache = true };
            return client;
        }

        public virtual async Task RunAsync()
        {
            for (var page = Source.PageMin; page <= Source.PageMax; ++page)
            {
                try
                {
                    await ExecuteAsync(page);
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Error while running page {page}", page);
                }
                Thread.Sleep(Source.Interval);
            }
        }

        protected virtual bool CanTwice() => true;

        protected async Task<bool> ExecuteAsync(int page)
        {
            if (!CanTwice() && 0 != Interlocked.Exchange(ref _executing, 1)) return false;
            var builder = new UriBuilder(UrlComposite(page));
            await Crawling(builder.Uri);
            Interlocked.Exchange(ref _executing, 0);
            return true;
        }

        protected abstract string UrlComposite(int page);

        private async Task Crawling(Uri uri)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            
            using var client = CreateHttpClient();
            var response = await client.GetAsync(uri.AbsoluteUri);
            if (response.StatusCode != HttpStatusCode.OK)
            {
                Log.Logger.Error("response failed. <Url:{UriAbsoluteUri}> <StatusCode:{StatusCode}>", uri.AbsoluteUri, response.StatusCode);
                return;
            }
            
            var responseBytes = await response.Content.ReadAsByteArrayAsync();

            string decodedString;
            try
            {
                decodedString = Encoding.UTF8.GetString(responseBytes);

                // 유효성 검사: UTF-8로 변환한 결과가 깨진 문자열인지 확인
                if (ContainsInvalidCharacters(decodedString))
                {
                    // 깨진 경우, EUC-KR로 재디코딩
                    Log.Logger.Error("Detected invalid UTF-8, switching to EUC-KR. <DecodedString:{decodedString}>", decodedString);

                    decodedString = Encoding.GetEncoding("EUC-KR").GetString(responseBytes);
                }
            }
            catch
            {
                // UTF-8 디코딩 자체가 실패하면 바로 EUC-KR 사용
                Log.Logger.Error("UTF-8 decoding failed, switching to EUC-KR.");

                decodedString = Encoding.GetEncoding("EUC-KR").GetString(responseBytes);
            }

            if (string.IsNullOrEmpty(decodedString))
            {
                Log.Logger.Error("Response content is null.");
                return;
            }

            var context = BrowsingContext.New(Configuration.Default.WithDefaultLoader());
            var document = await context.OpenAsync(req => req.Content(decodedString));
            
            OnPageCrawl(document);
        }

        private static bool ContainsInvalidCharacters(string text)
        {
            // 예시: 비정상적인 특수 문자 또는 ASCII 제어 문자가 많으면 깨진 문자열로 판단
            var invalidCharCount = text.Count(ch => char.IsControl(ch) && ch != '\n' && ch != '\r' && ch != '\t');

            // 특정 비율 이상의 제어 문자가 있으면 깨졌다고 판단 (임계값 조정 가능)
            return (float)invalidCharCount / text.Length > 0.1f;
        }

        protected abstract void OnPageCrawl(AngleSharp.Dom.IDocument document);

        protected virtual string UrlCompositeHref(string href)
        {
            return UrlBase.CutAndComposite("/", 0, 3, href);
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
