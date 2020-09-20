﻿using WebUtil.Services;
using System;
using Microsoft.Extensions.Logging;

namespace Server.Services
{
    public class FeedCrawlingRepeatedService : RepeatedService
    {
        private readonly FeedCrawlingService _crawlingService;

        private readonly ILogger<WebCrawlingRepeatedService> _logger;
        public FeedCrawlingRepeatedService(ILogger<WebCrawlingRepeatedService> logger, FeedCrawlingService crawlingService)
            : base(logger, new TimeSpan(0, 2, 0))
        {
            _crawlingService = crawlingService;
            _logger = logger;
        }

        protected override void DoWork(object state)
        {
            _ = _crawlingService.Execute(new Protocols.Request.Feed
            {
                All = true
            });
        }
    }
}
