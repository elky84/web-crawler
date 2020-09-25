using System.Text;
using System.Threading.Tasks;
using FeedCrawler.Crawler;
using MongoDB.Driver;
using Serilog;

namespace FeedCrawlerCli
{
    class Program
    {
        static async Task Main(string[] args)
        {
            System.Text.EncodingProvider provider = System.Text.CodePagesEncodingProvider.Instance;
            Encoding.RegisterProvider(provider);

            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .CreateLogger();

            Log.Logger.Information("starting up!");

            var client = new MongoClient("mongodb://localhost:27017/?maxPoolSize=200");
            var database = client.GetDatabase("cli-feed-crawler");

            await new RssCrawler(null, database, new FeedCrawler.Models.Rss
            {
                Url = "https://elky84.github.io/feed.xml",
                Name = "Elky Essay"
            }).RunAsync();

            await new RssCrawler(null, database, new FeedCrawler.Models.Rss
            {
                Url = "https://developer.amazon.com/blogs/home/feed/entries/atom",
                Name = "Amazon Developer Blogs"
            }).RunAsync();
        }
    }
}