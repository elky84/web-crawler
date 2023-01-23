using MongoDB.Driver;
using Serilog;
using System.Text;
using System.Threading.Tasks;
using WebCrawler.Code;
using WebCrawler.Crawler;

namespace Cli
{
    class Program
    {
        static async Task Main(string[] args)
        {
            EncodingProvider provider = System.Text.CodePagesEncodingProvider.Instance;
            Encoding.RegisterProvider(provider);

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Warning()
                .WriteTo.Console()
                .CreateLogger();

            Log.Warning("starting up!");

            var client = new MongoClient("mongodb://localhost:27017/?maxPoolSize=200");
            var database = client.GetDatabase("cli-web-crawler");

            await new FmkoreaCrawler(null, database, new WebCrawler.Models.Source
            {
                Type = CrawlingType.FmKorea,
                BoardId = "lol&sort_index=pop",
                Name = "펨코 롤"
            }).RunAsync();

            await new ItcmCrawler(null, database, new WebCrawler.Models.Source
            {
                Type = CrawlingType.Itcm,
                BoardId = "game_news",
                Name = "핫딜"
            }).RunAsync();

            await new ClienCrawler(null, database, new WebCrawler.Models.Source
            {
                Type = CrawlingType.Clien,
                BoardId = "jirum",
                Name = "지름"
            }).RunAsync();

            await new FmkoreaCrawler(null, database, new WebCrawler.Models.Source
            {
                Type = CrawlingType.FmKorea,
                BoardId = "hotdeal",
                Name = "펨코핫딜"
            }).RunAsync();

            await new FmkoreaCrawler(null, database, new WebCrawler.Models.Source
            {
                Type = CrawlingType.FmKorea,
                BoardId = "best",
                Name = "포텐터짐"
            }).RunAsync();

            await new HumorUnivCrawler(null, database, new WebCrawler.Models.Source
            {
                Type = CrawlingType.HumorUniv,
                BoardId = "pick",
                Name = "인기자료"
            }).RunAsync();

            await new InvenNewsCrawler(null, database, new WebCrawler.Models.Source
            {
                Type = CrawlingType.InvenNews,
                BoardId = "site=lol",
                Name = "인벤 LOL 뉴스"
            }).RunAsync();

            await new PpomppuCrawler(null, database, new WebCrawler.Models.Source
            {
                Type = CrawlingType.Ppomppu,
                BoardId = "freeboard",
                Name = "자유게시판"
            }).RunAsync();

            await new PpomppuCrawler(null, database, new WebCrawler.Models.Source
            {
                Type = CrawlingType.Ppomppu,
                BoardId = "ppomppu",
                Name = "뽐뿌핫딜"
            }).RunAsync();

            await new TodayhumorCrawler(null, database, new WebCrawler.Models.Source
            {
                Type = CrawlingType.TodayHumor,
                BoardId = "bestofbest",
                Name = "베오베"
            }).RunAsync();

            await new SlrclubCrawler(null, database, new WebCrawler.Models.Source
            {
                Type = CrawlingType.SlrClub,
                BoardId = "free",
                Name = "자유게시판"
            }).RunAsync();

            await new FmkoreaCrawler(null, database, new WebCrawler.Models.Source
            {
                Type = CrawlingType.FmKorea,
                BoardId = "football_news",
                Name = "축구 뉴스"
            }).RunAsync();

            await new ClienCrawler(null, database, new WebCrawler.Models.Source
            {
                Type = CrawlingType.Clien,
                BoardId = "sold",
                Name = "회원중고장터"
            }).RunAsync();

            await new RuliwebCrawler(null, database, new WebCrawler.Models.Source
            {
                Type = CrawlingType.Ruliweb,
                BoardId = "market/board/1020",
                Name = "핫딜게시판"
            }).RunAsync();

            await new RuliwebCrawler(null, database, new WebCrawler.Models.Source
            {
                Type = CrawlingType.Ruliweb,
                BoardId = "market/board/1003",
                Name = "콘솔뉴스"
            }).RunAsync();

            await new RuliwebCrawler(null, database, new WebCrawler.Models.Source
            {
                Type = CrawlingType.Ruliweb,
                BoardId = "best/selection",
                Name = "베스트"
            }).RunAsync();

            await new RuliwebCrawler(null, database, new WebCrawler.Models.Source
            {
                Type = CrawlingType.Ruliweb,
                BoardId = "best",
                Name = "유머베스트"
            }).RunAsync();
        }
    }
}