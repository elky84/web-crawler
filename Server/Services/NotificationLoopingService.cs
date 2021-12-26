using MongoDbWebUtil.Services;
using System;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using Server.Exception;

namespace Server.Services
{
    public class NotificationLoopingService : LoopingService
    {
        private readonly NotificationService _notificationService;

        private readonly ILogger<NotificationLoopingService> _logger;
        public NotificationLoopingService(ILogger<NotificationLoopingService> logger, NotificationService notificationService)
        {
            _notificationService = notificationService;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    _notificationService.HttpTaskRun();
                }
                catch (System.Exception e)
                {
                    e.ExceptionLog();
                }

                await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
            }
        }
    }
}
