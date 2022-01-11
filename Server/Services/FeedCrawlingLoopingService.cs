﻿using EzAspDotNet.Exception;
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

#pragma warning disable CS1998 // 이 비동기 메서드에는 'await' 연산자가 없으며 메서드가 동시에 실행됩니다.
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
#pragma warning restore CS1998 // 이 비동기 메서드에는 'await' 연산자가 없으며 메서드가 동시에 실행됩니다.
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    _crawlingService.Execute(new Protocols.Request.Feed { All = true }).Wait();
                }
                catch (System.Exception e)
                {
                    e.ExceptionLog();
                }

                await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
            }
        }
    }
}
