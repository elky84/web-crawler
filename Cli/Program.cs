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

            var clienCrawler = new ClienCrawler(database, "sold", 1);
            await clienCrawler.RunAsync();

            var ruliwebCrawler = new RuliwebCrawler(database, 1020, 1);
            await ruliwebCrawler.RunAsync();
        }
    }
}