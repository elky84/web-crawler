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
            : base(logger, new TimeSpan(0, 2, 0))
        {
            _crawlingService = crawlingService;
            _logger = logger;
        }

        protected override void DoWork(object state)
        {
#if DEBUG
#else
            _ = _crawlingService.Execute(new Protocols.Request.Crawling
            {
                All = true
            });
#endif
        }
    }
}
