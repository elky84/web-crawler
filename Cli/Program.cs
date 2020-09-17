using System.Text;
using System.Threading.Tasks;
using MongoDB.Driver;
using Serilog;
using WebCrawler;
using WebCrawler.Code;

namespace cli
{
    class Program
    {
        static async Task Main(string[] args)
        {
            System.Text.EncodingProvider provider = System.Text.CodePagesEncodingProvider.Instance;
            Encoding.RegisterProvider(provider);

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.Console()
                .CreateLogger();

            Log.Logger.Information("starting up!");

            var client = new MongoClient("mongodb://localhost:27017/?maxPoolSize=200");
            var database = client.GetDatabase("Cli-Web-Crawler");

            await new PpomppuCrawler(null, database, new WebCrawler.Models.Source
            {
                Type = CrawlingType.Ppomppu,
                BoardId = "freeboard",
                Name = "자유게시판"
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
                BoardId = "board/1020",
                Name = "핫딜게시판"
            }).RunAsync();
        }
    }
}