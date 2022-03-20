using EzAspDotNet.Util;
using System.Threading.Tasks;
using EzAspDotNet.Services;
using MongoDB.Driver;
using Server.Models;
using System.Collections.Generic;
using Server.Code;
using WebCrawler.Code;
using EzAspDotNet.Notification.Models;
using EzAspDotNet.Exception;
using EzAspDotNet.Notification.Types;

namespace Server.Services
{
    public class NotificationService
    {
        private readonly MongoDbUtil<Notification> _mongoDbNotification;

        private readonly SourceService _sourceService;

        public NotificationService(MongoDbService mongoDbService,
            SourceService sourceService)
        {
            _mongoDbNotification = new MongoDbUtil<Notification>(mongoDbService.Database);
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
            var created = await Create(notification.Data);

            return new Protocols.Response.Notification
            {
                ResultCode = ResultCode.Success,
                Data = created?.ToProtocol()
            };

        }

        private async Task<string> GetSourceId(CrawlingType crawlingType, string boardName)
        {
           var source = await _sourceService.GetByName(crawlingType, boardName);
            if (source == null)
            {
                throw new DeveloperException(ResultCode.NotFoundSource);
            }

            return source.Id;
        }

        private FilterDefinition<Notification> GetFilterDefinition(string sourceId, string crawlingType, NotificationType notificationType)
        {
            return Builders<Notification>.Filter.Eq(x => x.SourceId, sourceId) &
                    Builders<Notification>.Filter.Eq(x => x.CrawlingType, crawlingType) &
                    Builders<Notification>.Filter.Eq(x => x.Type, notificationType);
        }

        private async Task<Notification> Create(Protocols.Common.NotificationCreate notification)
        {
            try
            {
                var sourceId = await GetSourceId(notification.CrawlingType, notification.BoardName);
                return await _mongoDbNotification.UpsertAsync(GetFilterDefinition(sourceId, notification.CrawlingType.ToString(), notification.Type),
                    notification.ToModel(sourceId));
            }
            catch (MongoWriteException)
            {
                throw new DeveloperException(ResultCode.UsingNotificationId);
            }
        }

        public async Task<Protocols.Response.NotificationMulti> CreateMulti(Protocols.Request.NotificationMulti notificationMulti)
        {
            var notifications = new List<Notification>();
            foreach (var notification in notificationMulti.Datas)
            {
                notifications.Add(await Create(notification));
            }

            return new Protocols.Response.NotificationMulti
            {
                Datas = notifications.ConvertAll(x => x.ToProtocol())
            };
        }

        public async Task<Protocols.Response.Notification> Get(string id)
        {
            return new Protocols.Response.Notification
            {
                ResultCode = ResultCode.Success,
                Data = (await _mongoDbNotification.FindOneAsyncById(id))?.ToProtocol()
            };
        }

        public async Task<List<Notification>> Get(FilterDefinition<Notification> filter)
        {
            return await _mongoDbNotification.FindAsync(filter);
        }

        public async Task<Protocols.Response.Notification> Update(Protocols.Request.NotificationUpdate notificationUpdate)
        {
            var update = notificationUpdate.Data.ToModel();

            var filter = GetFilterDefinition(update.SourceId, update.CrawlingType, update.Type);
            var updated = await _mongoDbNotification.UpsertAsync(filter, update);
            return new Protocols.Response.Notification
            {
                ResultCode = ResultCode.Success,
                Data = (updated ?? update).ToProtocol()
            };
        }

        public async Task<Protocols.Response.Notification> Update(string id, Protocols.Request.NotificationUpdate notificationUpdate)
        {
            var update = notificationUpdate.Data.ToModel();

            var updated = await _mongoDbNotification.UpdateAsync(id, update);
            return new Protocols.Response.Notification
            {
                ResultCode = ResultCode.Success,
                Data = (updated ?? update).ToProtocol()
            };
        }

        public async Task<Protocols.Response.Notification> Delete(string id)
        {
            return new Protocols.Response.Notification
            {
                ResultCode = ResultCode.Success,
                Data = (await _mongoDbNotification.RemoveGetAsync(id))?.ToProtocol()
            };
        }
    }
}
