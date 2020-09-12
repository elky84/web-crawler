using System.Text;
using System.Threading.Tasks;
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

            var clienCrawler = new ClienCrawler(null, "sold", 1);
            await clienCrawler.RunAsync();

            var ruliwebCrawler = new RuliwebCrawler(null, 1020, 1);
            await ruliwebCrawler.RunAsync();
        }
    }
}