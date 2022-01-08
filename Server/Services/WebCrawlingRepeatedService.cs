using MongoDbWebUtil.Services;
using System;
using Microsoft.Extensions.Logging;
using EzAspDotNet.Exception;

namespace Server.Services
{
    public class WebCrawlingRepeatedService : RepeatedService
    {
        private readonly WebCrawlingService _crawlingService;

        private readonly ILogger<WebCrawlingRepeatedService> _logger;
        public WebCrawlingRepeatedService(ILogger<WebCrawlingRepeatedService> logger, WebCrawlingService crawlingService)
            : base(logger, new TimeSpan(0, 2, 0))
        {
            _crawlingService = crawlingService;
            _logger = logger;
        }

        protected override void DoWork(object state)
        {
            try
            {
                _crawlingService.Execute(new Protocols.Request.Crawling { All = true }).Wait();
            }
            catch (System.Exception e)
            {
                e.ExceptionLog();
            }
        }
    }
}
