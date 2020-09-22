using WebUtil.Util;
using System.Threading.Tasks;
using WebUtil.Services;
using MongoDB.Driver;
using Server.Models;
using System.Collections.Generic;
using Server.Exception;
using System.Net.Http;
using WebCrawler.Models;
using FeedCrawler.Models;
using System.Linq;
using System;
using System.Collections.Concurrent;
using Server.Code;
using System.Threading;
using Serilog;

namespace Server.Services
{
    public class NotificationService
    {
        private readonly MongoDbUtil<Notification> _mongoDbNotification;


        private readonly IHttpClientFactory _httpClientFactory;

        private readonly SourceService _sourceService;


        private readonly List<Protocols.Notification.Request.DiscordWebHook> _discordWebHooks =
            new List<Protocols.Notification.Request.DiscordWebHook>();

        private readonly List<Protocols.Notification.Request.SlackWebHook> _slackWebHooks =
            new List<Protocols.Notification.Request.SlackWebHook>();

        public NotificationService(MongoDbService mongoDbService,
            IHttpClientFactory httpClientFactory,
            SourceService sourceService)
        {
            _mongoDbNotification = new MongoDbUtil<Notification>(mongoDbService.Database);
            _httpClientFactory = httpClientFactory;

            _sourceService = sourceService;
        }

        public async Task<List<Notification>> All()
        {
            return await _mongoDbNotification.All();
        }

        public async Task<Protocols.Response.Notification> Create(Protocols.Request.NotificationCreate notification)
        {
            var created = await Create(notification.NotificationData);

            return new Protocols.Response.Notification
            {
                ResultCode = ResultCode.Success,
                NotificationData = created?.ToProtocol()
            };

        }

        private async Task<string> GetSourceId(Protocols.Common.NotificationCreate notification)
        {
            if (notification.CrawlingType == WebCrawler.Code.CrawlingType.Rss)
            {
                return string.Empty;
            }

            var source = await _sourceService.GetByName(notification.CrawlingType, notification.BoardName);
            if (source == null)
            {
                throw new DeveloperException(Code.ResultCode.NotFoundSource);
            }

            return source.Id;
        }

        private async Task<Notification> Create(Protocols.Common.NotificationCreate notification)
        {
            try
            {
                var sourceId = await GetSourceId(notification);
                var newData = notification.ToModel(sourceId);

                var origin = await _mongoDbNotification.FindOneAsync(Builders<Notification>.Filter.Eq(x => x.SourceId, sourceId) &
                    Builders<Notification>.Filter.Eq(x => x.CrawlingType, notification.CrawlingType) &
                    Builders<Notification>.Filter.Eq(x => x.Type, notification.Type));
                if (origin != null)
                {
                    newData.Id = origin.Id;
                    await _mongoDbNotification.UpdateAsync(newData.Id, newData);
                    return newData;
                }
                else
                {
                    return await _mongoDbNotification.CreateAsync(newData);
                }
            }
            catch (MongoWriteException)
            {
                throw new DeveloperException(Code.ResultCode.UsingNotificationId);
            }
        }

        public async Task<Protocols.Response.NotificationMulti> CreateMulti(Protocols.Request.NotificationMulti notificationMulti)
        {
            var notifications = new List<Notification>();
            foreach (var notification in notificationMulti.NotificationDatas)
            {
                notifications.Add(await Create(notification));
            }

            return new Protocols.Response.NotificationMulti
            {
                NotificationDatas = notifications.ConvertAll(x => x.ToProtocol())
            };
        }

        public async Task<Protocols.Response.Notification> Get(string id)
        {
            return new Protocols.Response.Notification
            {
                ResultCode = ResultCode.Success,
                NotificationData = (await _mongoDbNotification.FindOneAsyncById(id))?.ToProtocol()
            };
        }

        public async Task<List<Notification>> Get(FilterDefinition<Notification> filter)
        {
            return await _mongoDbNotification.FindAsync(filter);
        }

        public async Task<Protocols.Response.Notification> Update(string id, Protocols.Request.NotificationUpdate notificationUpdate)
        {
            var update = notificationUpdate.NotificationData.ToModel();

            var updated = await _mongoDbNotification.UpdateAsync(id, update);
            return new Protocols.Response.Notification
            {
                ResultCode = ResultCode.Success,
                NotificationData = (updated ?? update).ToProtocol()
            };
        }

        public async Task<Protocols.Response.Notification> Delete(string id)
        {
            return new Protocols.Response.Notification
            {
                ResultCode = ResultCode.Success,
                NotificationData = (await _mongoDbNotification.RemoveAsync(id))?.ToProtocol()
            };
        }

        public async Task Execute(FilterDefinition<Notification> filter, CrawlingData crawlingData)
        {
            var notifications = await Get(filter);
            foreach (var notification in notifications)
            {
                if (!string.IsNullOrEmpty(notification.Keyword) && !crawlingData.Title.Contains(notification.Keyword))
                {
                    return;
                }

                switch (notification.Type)
                {
                    case NotificationType.Discord:
                        _discordWebHooks.Add(DiscordNotify(notification, crawlingData));
                        break;
                    case NotificationType.Slack:
                        _slackWebHooks.Add(SlackNotify(notification, crawlingData));
                        break;
                    default:
                        throw new DeveloperException(ResultCode.NotImplementedYet);
                }
            }
        }


        public async Task Execute(FilterDefinition<Notification> filter, FeedData feedData)
        {
            var notifications = await Get(filter);
            foreach (var notification in notifications)
            {
                if (!string.IsNullOrEmpty(notification.Keyword) &&
                    !feedData.FeedTitle.Contains(notification.Keyword) &&
                    !feedData.ItemTitle.Contains(notification.Keyword))
                {
                    return;
                }

                switch (notification.Type)
                {
                    case NotificationType.Discord:
                        _discordWebHooks.Add(DiscordNotify(notification, feedData));
                        break;
                    case NotificationType.Slack:
                        _slackWebHooks.Add(SlackNotify(notification, feedData));
                        break;

                    default:
                        throw new DeveloperException(ResultCode.NotImplementedYet);
                }
            }
        }


        private Protocols.Notification.Request.SlackWebHook SlackNotify(Notification notification, CrawlingData crawlingData)
        {
            var category = string.IsNullOrEmpty(crawlingData.Category) ? string.Empty : $"[{crawlingData.Category}]";
            return new Protocols.Notification.Request.SlackWebHook
            {
                username = notification.Name,
                channel = notification.Channel,
                icon_url = notification.IconUrl,
                text = $"<{crawlingData.Href}|{category}{crawlingData.Title}> [{crawlingData.DateTime}]",
                HookUrl = notification.HookUrl
            };
        }

        private Protocols.Notification.Request.DiscordWebHook DiscordNotify(Notification notification, CrawlingData crawlingData)
        {
            var category = string.IsNullOrEmpty(crawlingData.Category) ? string.Empty : $"<{crawlingData.Category}>";
            return new Protocols.Notification.Request.DiscordWebHook
            {
                username = notification.Name,
                avatar_url = notification.IconUrl,
                content = $"[{category}{crawlingData.Title}]({crawlingData.Href}) <{crawlingData.DateTime}>",
                HookUrl = notification.HookUrl
            };
        }



        private Protocols.Notification.Request.SlackWebHook SlackNotify(Notification notification, FeedData feedData)
        {
            return new Protocols.Notification.Request.SlackWebHook
            {
                username = notification.Name,
                channel = notification.Channel,
                icon_url = notification.IconUrl,
                text = $"<{feedData.Href}|<{feedData.FeedTitle}>[{feedData.ItemTitle}]> [{feedData.DateTime}]",
                HookUrl = notification.HookUrl
            };
        }

        private Protocols.Notification.Request.DiscordWebHook DiscordNotify(Notification notification, FeedData feedData)
        {
            return new Protocols.Notification.Request.DiscordWebHook
            {
                username = notification.Name,
                avatar_url = notification.IconUrl,
                content = $"[<{feedData.FeedTitle}>{feedData.ItemTitle}]({feedData.Href}) <{feedData.DateTime}>",
                HookUrl = notification.HookUrl
            };
        }

        private async Task ProcessSlackWebHooks()
        {
            var processList = new List<Protocols.Notification.Request.SlackWebHook>();
            foreach (var group in _slackWebHooks.GroupBy(x => x.channel))
            {
                foreach (var webHook in group.Select(x => x))
                {
                    await _httpClientFactory.RequestJson(HttpMethod.Post, webHook.HookUrl, webHook);
                    Thread.Sleep(1000); // 초당 한개...라고함.
                    processList.Add(webHook);
                }
            }

            foreach (var process in processList)
            {
                _slackWebHooks.Remove(process);
            }
        }

        public static int UnixTimeNow()
        {
            return (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
        }

        private async Task ProcessDiscordWebHooks()
        {
            var processList = new List<Protocols.Notification.Request.DiscordWebHook>();
            foreach (var group in _discordWebHooks.GroupBy(x => x.HookUrl))
            {
                foreach (var webHook in group.Select(x => x))
                {
                    Log.Logger.Error($"Try Request [{group.Key}]");
                    var response = await _httpClientFactory.RequestJson(HttpMethod.Post, group.Key, webHook);
                    if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                    {
                        Log.Logger.Error($"Too Many Requests [{group.Key}]");
                        break;
                    }

                    processList.Add(webHook);

                    var rateLimitRemaining = response.Headers.GetValues("x-ratelimit-remaining").FirstOrDefault().ToInt();
                    if (rateLimitRemaining == 0)
                    {
                        var waitTime = Math.Max(0, response.Headers.GetValues("x-ratelimit-reset").FirstOrDefault().ToInt() - UnixTimeNow()) + 1;
                        Log.Logger.Error($"RateLimiting Sleep [{group.Key}, {waitTime}]");
                        await Task.Delay(waitTime * 1000);
                        break;
                    }
                }
            }

            foreach (var process in processList)
            {
                _discordWebHooks.Remove(process);
            }
        }

        public async Task HttpTaskRun()
        {
            await ProcessDiscordWebHooks();
            await ProcessSlackWebHooks();
        }
    }
}
