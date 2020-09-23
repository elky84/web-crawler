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

            _mongoDbNotification.Collection.Indexes.CreateOne(new CreateIndexModel<Notification>(
                Builders<Notification>.IndexKeys.Ascending(x => x.SourceId)
                .Ascending(x => x.CrawlingType)
                .Ascending(x => x.Type)));
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
                throw new DeveloperException(ResultCode.NotFoundSource);
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
                if (!notification.ContainsKeyword(crawlingData.Title))
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
                if (!notification.ContainsKeyword(feedData.FeedTitle) &&
                    !notification.ContainsKeyword(feedData.ItemTitle))
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

        private void ProcessSlackWebHooks()
        {
            var processList = new ConcurrentBag<Protocols.Notification.Request.SlackWebHook>();
            Parallel.ForEach(_slackWebHooks.GroupBy(x => x.channel), group =>
            {
                foreach (var webHook in group.Select(x => x))
                {
                    _ = _httpClientFactory.RequestJson(HttpMethod.Post, webHook.HookUrl, webHook);
                    Thread.Sleep(1000); // 초당 한개...라고함.
                    processList.Add(webHook);
                }
            });

            foreach (var process in processList)
            {
                _slackWebHooks.Remove(process);
            }
        }

        private void ProcessDiscordWebHooks()
        {
            var processList = new ConcurrentBag<Protocols.Notification.Request.DiscordWebHook>();
            Parallel.ForEach(_discordWebHooks.GroupBy(x => x.HookUrl), group =>
            {
                foreach (var webHook in group.Select(x => x))
                {
                    var response = _httpClientFactory.RequestJson(HttpMethod.Post, group.Key, webHook).Result;
                    var rateLimitRemaining = response.Headers.GetValues("x-ratelimit-remaining").FirstOrDefault().ToInt();
                    var rateLimitAfter = response.Headers.GetValues("x-ratelimit-reset-after").FirstOrDefault().ToInt();
                    if (response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        processList.Add(webHook);
                    }

                    if (rateLimitRemaining <= 1 || rateLimitAfter > 0)
                    {
                        Thread.Sleep((rateLimitAfter + 1) * 1000);
                        continue;
                    }

                    if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                    {
                        Log.Logger.Error($"Too Many Requests [{group.Key}] [{rateLimitRemaining}, {rateLimitAfter}]");
                        break;
                    }
                }
            });

            foreach (var process in processList)
            {
                _discordWebHooks.Remove(process);
            }
        }

        public void HttpTaskRun()
        {
            ProcessDiscordWebHooks();
            ProcessSlackWebHooks();
        }
    }
}
