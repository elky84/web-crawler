using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Server.Models
{
    public static class ModelsExtend
    {
        public static Protocols.Common.Source ToProtocol(this WebCrawler.Models.Source source)
        {
            return new Protocols.Common.Source
            {
                Id = source.Id,
                BoardId = source.BoardId,
                Type = source.Type,
                Name = source.Name,
                PageMin = source.PageMin,
                PageMax = source.PageMax,
                Interval = source.Interval
            };
        }

        public static WebCrawler.Models.Source ToModel(this Protocols.Common.Source source)
        {
            return new WebCrawler.Models.Source
            {
                Id = source.Id,
                BoardId = source.BoardId,
                Type = source.Type,
                Name = source.Name,
                PageMin = source.PageMin,
                PageMax = source.PageMax,
                Interval = source.Interval
            };
        }


        public static Protocols.Common.Notification ToProtocol(this Notification notification)
        {
            return new Protocols.Common.Notification
            {
                Id = notification.Id,
                Type = notification.Type,
                Name = notification.Name,
                HookUrl = notification.HookUrl,
                Channel = notification.Channel,
                IconUrl = notification.IconUrl,
                SourceId = notification.SourceId,
                CountBaseLine = notification.CountBaseLine,
                RecommendBaseLine = notification.RecommendBaseLine
            };
        }

        public static Notification ToModel(this Protocols.Common.Notification notification)
        {
            return new Notification
            {
                Id = notification.Id,
                Type = notification.Type,
                Name = notification.Name,
                HookUrl = notification.HookUrl,
                Channel = notification.Channel,
                IconUrl = notification.IconUrl,
                SourceId = notification.SourceId,
                CountBaseLine = notification.CountBaseLine,
                RecommendBaseLine = notification.RecommendBaseLine
            };
        }

        public static Notification ToModel(this Protocols.Common.NotificationCreate notification, string sourceId)
        {
            return new Notification
            {
                Type = notification.Type,
                Name = notification.Name,
                HookUrl = notification.HookUrl,
                Channel = notification.Channel,
                IconUrl = notification.IconUrl,
                SourceId = sourceId,
                CountBaseLine = notification.CountBaseLine,
                RecommendBaseLine = notification.RecommendBaseLine
            };
        }


        public static Protocols.Common.CrawlingData ToProtocol(this WebCrawler.Models.CrawlingData crawling)
        {
            return new Protocols.Common.CrawlingData
            {
                Id = crawling.Id,
                Type = crawling.Type,
                BoardId = crawling.BoardId,
                BoardName = crawling.BoardName,
                Author = crawling.Author,
                SourceId = crawling.SourceId,
                Href = crawling.Href,
                Category = crawling.Category,
                Count = crawling.Count,
                Recommend = crawling.Recommend,
                Title = crawling.Title,
                DateTime = crawling.DateTime,
                RowId = crawling.RowId
            };
        }

        public static WebCrawler.Models.CrawlingData ToModel(this Protocols.Common.CrawlingData crawling)
        {
            return new WebCrawler.Models.CrawlingData
            {
                Id = crawling.Id,
                Type = crawling.Type,
                BoardId = crawling.BoardId,
                BoardName = crawling.BoardName,
                Author = crawling.Author,
                SourceId = crawling.SourceId,
                Href = crawling.Href,
                Category = crawling.Category,
                Count = crawling.Count,
                Recommend = crawling.Recommend,
                Title = crawling.Title,
                DateTime = crawling.DateTime,
                RowId = crawling.RowId
            };
        }

    }
}
