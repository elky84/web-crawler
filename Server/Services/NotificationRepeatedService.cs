using WebUtil.Services;
using System;
using Microsoft.Extensions.Logging;
using System.Threading;

namespace Server.Services
{
    public class NotificationRepeatedService : RepeatedService
    {
        private readonly NotificationService _notificationService;

        private readonly ILogger<NotificationRepeatedService> _logger;
        public NotificationRepeatedService(ILogger<NotificationRepeatedService> logger, NotificationService notificationService)
            : base(logger, new TimeSpan(0, 0, 3))
        {
            _notificationService = notificationService;
            _logger = logger;
        }

        protected override void DoWork(object state)
        {
            // 임의로 Discord 제한에 맞춤
            _ = _notificationService.HttpTaskRun(5);
        }
    }
}
