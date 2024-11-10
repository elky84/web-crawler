using EzAspDotNet.Exception;
using EzAspDotNet.Services;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Server.Services
{
    public class CrawlingLoopingService(CrawlingService crawlingService) : LoopingService
    {

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await crawlingService.Execute(new Protocols.Request.Crawling { All = true });
                }
                catch (Exception e)
                {
                    e.ExceptionLog();
                }

                await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
            }
        }
    }
}
