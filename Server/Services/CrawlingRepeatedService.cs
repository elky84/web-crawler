using WebUtil.Services;
using System;
using Microsoft.Extensions.Logging;

namespace Server.Services
{
    public class CrawlingRepeatedService : RepeatedService
    {
        private readonly CrawlingService _crawlingService;

        private readonly ILogger<CrawlingRepeatedService> _logger;
        public CrawlingRepeatedService(ILogger<CrawlingRepeatedService> logger, CrawlingService crawlingService)
            : base(logger, new TimeSpan(0, 5, 0))
        {
            _crawlingService = crawlingService;
            _logger = logger;
        }

        protected override void DoWork(object state)
        {
            var now = DateTime.Now;

            var openTime = now.Date.AddHours(9);
            var closeTime = now.Date.AddHours(15);

            if (openTime >= now && closeTime <= now)
            {
                _ = _crawlingService.Execute(new Protocols.Request.Crawling
                {
                    Page = 1,
                    All = true
                });
            }
        }
    }
}
