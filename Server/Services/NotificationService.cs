using EzAspDotNet.Exception;
using EzAspDotNet.Models;
using EzAspDotNet.Notification.Models;
using EzAspDotNet.Notification.Types;
using EzAspDotNet.Services;
using EzMongoDb.Util;
using MongoDB.Driver;
using Server.Code;
using System.Collections.Generic;
using System.Threading.Tasks;
using WebCrawler.Code;

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
            if (created == null)
            {
                throw new DeveloperException(Code.ResultCode.CreateFailedNotification);
            }

            return new Protocols.Response.Notification
            {
                ResultCode = ResultCode.Success,
                Data = MapperUtil.Map<Protocols.Common.Notification>(created)
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

        private static FilterDefinition<Notification> GetFilterDefinition(string sourceId, string crawlingType, NotificationType notificationType)
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
                var notificationModel = MapperUtil.Map<Notification>(notification);
                notificationModel.SourceId = sourceId;

                return await _mongoDbNotification.CreateAsync(notificationModel);
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
                Datas = MapperUtil.Map<List<Notification>, List<Protocols.Common.Notification>>(notifications)
            };
        }

        public async Task<Protocols.Response.Notification> Get(string id)
        {
            var notification = await _mongoDbNotification.FindOneAsyncById(id);
            if (notification == null)
            {
                throw new DeveloperException(Code.ResultCode.NotFoundData);
            }

            return new Protocols.Response.Notification
            {
                ResultCode = ResultCode.Success,
                Data = MapperUtil.Map<Protocols.Common.Notification>(notification)
            };
        }

        public async Task<List<Notification>> Get(FilterDefinition<Notification> filter)
        {
            return await _mongoDbNotification.FindAsync(filter);
        }

        public async Task<Protocols.Response.Notification> Update(Protocols.Request.NotificationUpdate notificationUpdate)
        {
            var update = MapperUtil.Map<Notification>(notificationUpdate.Data);
            if (update == null)
            {
                throw new DeveloperException(ResultCode.InvalidRequest);
            }

            var filter = GetFilterDefinition(update.SourceId, update.CrawlingType, update.Type);
            var updated = await _mongoDbNotification.UpsertAsync(filter, update);
            return new Protocols.Response.Notification
            {
                ResultCode = ResultCode.Success,
                Data = MapperUtil.Map<Protocols.Common.Notification>(updated)
            };
        }

        public async Task<Protocols.Response.Notification> Update(string id, Protocols.Request.NotificationUpdate notificationUpdate)
        {
            var update = MapperUtil.Map<Notification>(notificationUpdate.Data);
            if (update == null)
            {
                throw new DeveloperException(ResultCode.InvalidRequest);
            }

            var updated = await _mongoDbNotification.UpdateAsync(id, update);
            return new Protocols.Response.Notification
            {
                ResultCode = ResultCode.Success,
                Data = MapperUtil.Map<Protocols.Common.Notification>(updated)
            };
        }

        public async Task<Protocols.Response.Notification> Delete(string id)
        {
            var deleted = await _mongoDbNotification.RemoveGetAsync(id);
            if (deleted == null)
            {
                throw new DeveloperException(ResultCode.NotFoundData);
            }

            return new Protocols.Response.Notification
            {
                ResultCode = ResultCode.Success,
                Data = MapperUtil.Map<Protocols.Common.Notification>(deleted)
            };
        }
    }
}
