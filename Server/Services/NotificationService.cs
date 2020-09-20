using WebUtil.Util;
using System.Threading.Tasks;
using WebUtil.Services;
using MongoDB.Driver;
using Server.Models;
using System.Collections.Generic;
using Server.Exception;
using System.Net.Http;
using WebUtil.HttpClient;
using WebCrawler.Models;

namespace Server.Services
{
    public class NotificationService
    {
        private readonly MongoDbUtil<Notification> _mongoDbNotification;

        private readonly IHttpClientFactory _httpClientFactory;

        private readonly SourceService _sourceService;

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
                ResultCode = Code.ResultCode.Success,
                NotificationData = created?.ToProtocol()
            };

        }

        private async Task<Notification> Create(Protocols.Common.NotificationCreate notification)
        {
            try
            {
                var source = await _sourceService.GetByName(notification.CrawlingType, notification.BoardName);
                if (source == null)
                {
                    throw new DeveloperException(Code.ResultCode.NotFoundSource);
                }

                var newData = notification.ToModel(source.Id);

                var origin = await _mongoDbNotification.FindOneAsync(Builders<Notification>.Filter.Eq(x => x.SourceId, source.Id) &
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
                ResultCode = Code.ResultCode.Success,
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
                ResultCode = Code.ResultCode.Success,
                NotificationData = (updated ?? update).ToProtocol()
            };
        }

        public async Task<Protocols.Response.Notification> Delete(string id)
        {
            return new Protocols.Response.Notification
            {
                ResultCode = Code.ResultCode.Success,
                NotificationData = (await _mongoDbNotification.RemoveAsync(id))?.ToProtocol()
            };
        }

        public async Task Execute(FilterDefinition<Notification> filter, CrawlingData crawlingData)
        {
            var notifications = await Get(filter);
            foreach (var notification in notifications)
            {
                switch (notification.Type)
                {
                    case Code.NotificationType.Slack:
                        await SlackNotify(notification, crawlingData);
                        break;
                    case Code.NotificationType.Discord:
                        await DiscordNotify(notification, crawlingData);
                        break;
                    default:
                        throw new DeveloperException(Code.ResultCode.NotImplementedYet);
                }
            }
        }

        private async Task SlackNotify(Notification notification, CrawlingData crawlingData)
        {
            var category = string.IsNullOrEmpty(crawlingData.Category) ? string.Empty : $"[{crawlingData.Category}]";

            await _httpClientFactory.RequestJson<Protocols.Notification.Response.SlackWebHook>(HttpMethod.Post,
                notification.HookUrl,
                new Protocols.Notification.Request.SlackWebHook
                {
                    username = notification.Name,
                    channel = notification.Channel,
                    icon_url = notification.IconUrl,
                    text = $"<{crawlingData.Href}|{category}{crawlingData.Title}> [{crawlingData.DateTime}]"
                }
            );
        }

        private async Task DiscordNotify(Notification notification, CrawlingData crawlingData)
        {
            var category = string.IsNullOrEmpty(crawlingData.Category) ? string.Empty : $"<{crawlingData.Category}>";

            await _httpClientFactory.RequestJson<Protocols.Notification.Response.DiscordWebHook>(HttpMethod.Post,
                notification.HookUrl,
                new Protocols.Notification.Request.DiscordWebHook
                {
                    username = notification.Name,
                    avatar_url = notification.IconUrl,
                    content = $"[{category}{crawlingData.Title}]({crawlingData.Href}) <{crawlingData.DateTime}>"
                }
            );
        }
    }
}
