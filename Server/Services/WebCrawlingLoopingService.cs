using EzAspDotNet.Exception;
using System.Threading;
using System.Threading.Tasks;
using EzAspDotNet.Services;
using System;

namespace Server.Services
{
    public class WebCrawlingLoopingService : LoopingService
    {
        private readonly WebCrawlingService _crawlingService;

        public WebCrawlingLoopingService(WebCrawlingService crawlingService)
        {
            _crawlingService = crawlingService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        { 
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await _crawlingService.Execute(new Protocols.Request.Crawling { All = true });
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
