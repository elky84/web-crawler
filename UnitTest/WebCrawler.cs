using MongoDB.Driver;
using NUnit.Framework;
using Serilog;
using System.Text;
using WebCrawler.Code;
using WebCrawler.Crawler;

namespace UnitTest
{
    public class WebCrawlerTests
    {
        public IMongoDatabase? Database { get; set; }

        [SetUp]
        public void Setup()
        {
            EncodingProvider provider = CodePagesEncodingProvider.Instance;
            Encoding.RegisterProvider(provider);

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.Console()
                .CreateLogger();

            Log.Logger.Information("starting up!");

            var client = new MongoClient("mongodb://localhost:27017/?maxPoolSize=200");
            Database = client.GetDatabase("cli-web-crawler");
        }

        [Test]
        public void Itcm()
        {
            new ItcmCrawler(null, Database, new WebCrawler.Models.Source
            {
                Type = CrawlingType.Itcm,
                BoardId = "game_news",
                Name = "핫딜"
            }).RunAsync().Wait();
        }

        [Test]
        public void ClientJirum()
        { 
            new ClienCrawler(null, Database, new WebCrawler.Models.Source
            {
                Type = CrawlingType.Clien,
                BoardId = "jirum",
                Name = "지름"
            }).RunAsync().Wait();
        }

        [Test]
        public void FmKoreaHotdeal()
        {
            new FmkoreaCrawler(null, Database, new WebCrawler.Models.Source
            {
                Type = CrawlingType.FmKorea,
                BoardId = "hotdeal",
                Name = "펨코핫딜"
            }).RunAsync().Wait();
        }

        [Test]
        public void FmKoreaBest()
        {
            new FmkoreaCrawler(null, Database, new WebCrawler.Models.Source
            {
                Type = CrawlingType.FmKorea,
                BoardId = "best",
                Name = "포텐터짐"
            }).RunAsync().Wait();
        }

        [Test]
        public void HumorUnivBest()
        {

            new HumorUnivCrawler(null, Database, new WebCrawler.Models.Source
            {
                Type = CrawlingType.HumorUniv,
                BoardId = "pick",
                Name = "인기자료"
            }).RunAsync().Wait();
        }

        [Test]
        public void InvenLOL()
        {

            new InvenNewsCrawler(null, Database, new WebCrawler.Models.Source
            {
                Type = CrawlingType.InvenNews,
                BoardId = "site=lol",
                Name = "인벤 LOL 뉴스"
            }).RunAsync().Wait();
        }

        [Test]
        public void PpomppuFree()
        {

            new PpomppuCrawler(null, Database, new WebCrawler.Models.Source
            {
                Type = CrawlingType.Ppomppu,
                BoardId = "freeboard",
                Name = "자유게시판"
            }).RunAsync().Wait();
        }

        [Test]
        public void TodayHumorBestOfBest()
        {
            new TodayhumorCrawler(null, Database, new WebCrawler.Models.Source
            {
                Type = CrawlingType.TodayHumor,
                BoardId = "bestofbest",
                Name = "베오베"
            }).RunAsync().Wait();
        }

        [Test]
        public void SlrClubFree()
        {

            new SlrclubCrawler(null, Database, new WebCrawler.Models.Source
            {
                Type = CrawlingType.SlrClub,
                BoardId = "free",
                Name = "자유게시판"
            }).RunAsync().Wait();
        }

        [Test]
        public void FmKoreaFootballNews()
        {
            new FmkoreaCrawler(null, Database, new WebCrawler.Models.Source
            {
                Type = CrawlingType.FmKorea,
                BoardId = "football_news",
                Name = "축구 뉴스"
            }).RunAsync().Wait();
        }

        [Test]
        public void ClienSold()
        {
            new ClienCrawler(null, Database, new WebCrawler.Models.Source
            {
                Type = CrawlingType.Clien,
                BoardId = "sold",
                Name = "회원중고장터"
            }).RunAsync().Wait();
        }

        [Test]
        public void RuliwebHotdeal()
        {

            new RuliwebCrawler(null, Database, new WebCrawler.Models.Source
            {
                Type = CrawlingType.Ruliweb,
                BoardId = "market/board/1020",
                Name = "핫딜게시판"
            }).RunAsync().Wait();
        }

        [Test]
        public void RuliwebConsoleNews()
        {
            new RuliwebCrawler(null, Database, new WebCrawler.Models.Source
            {
                Type = CrawlingType.Ruliweb,
                BoardId = "market/board/1003",
                Name = "콘솔뉴스"
            }).RunAsync().Wait();
        }

        [Test]
        public void RuliwebBest()
        {

            new RuliwebCrawler(null, Database, new WebCrawler.Models.Source
            {
                Type = CrawlingType.Ruliweb,
                BoardId = "best/selection",
                Name = "베스트"
            }).RunAsync().Wait();
        }

        [Test]
        public void RuliwebHumorBest()
        {
            new RuliwebCrawler(null, Database, new WebCrawler.Models.Source
            {
                Type = CrawlingType.Ruliweb,
                BoardId = "best",
                Name = "유머베스트"
            }).RunAsync().Wait();
        }
    }
}