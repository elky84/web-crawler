using EzAspDotNet.Exception;
using System.Threading.Tasks;
using System.Threading;
using EzAspDotNet.Services;
using System;

namespace Server.Services
{
    public class FeedCrawlingLoopingService : LoopingService
    {
        private readonly FeedCrawlingService _crawlingService;

        public FeedCrawlingLoopingService(FeedCrawlingService crawlingService)
        {
            _crawlingService = crawlingService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await _crawlingService.Execute(new Protocols.Request.Feed { All = true });
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
