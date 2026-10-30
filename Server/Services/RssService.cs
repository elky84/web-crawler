using EzAspDotNet.Services;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;
using WebCrawler.Code;
using WebCrawler.Models;

namespace Server.Services
{
    public enum RssFlavor
    {
        // 크롤링 데이터 그대로
        Rss,
        // 짧은 형태 ([카테고리] 제목 / 작성자)
        Short
    }

    public class RssQuery
    {
        public const int DefaultLimit = 200;

        public const int MaxLimit = 5000;

        public DateTime? From { get; set; }

        public DateTime? To { get; set; }

        public CrawlingType? Type { get; set; }

        public string BoardId { get; set; }

        public string Keyword { get; set; }

        public int Limit { get; set; } = DefaultLimit;
    }

    public class RssService
    {
        private readonly IMongoCollection<CrawlingData> _collection;

        public RssService(MongoDbService mongoDbService)
        {
            _collection = mongoDbService.Database.GetCollection<CrawlingData>(typeof(CrawlingData).Name);
        }

        // Kind 가 Unspecified 인 입력은 서버 로컬 시간으로 간주해 저장값(UTC)과 같은 기준으로 맞춘다
        private static DateTime ToUtc(DateTime value) =>
            value.Kind == DateTimeKind.Utc ? value : TimeZoneInfo.ConvertTimeToUtc(
                value.Kind == DateTimeKind.Local ? value : DateTime.SpecifyKind(value, DateTimeKind.Local));

        public async Task<List<CrawlingData>> Find(RssQuery query)
        {
            var builder = Builders<CrawlingData>.Filter;
            var filter = FilterDefinition<CrawlingData>.Empty;

            if (query.From.HasValue)
            {
                filter &= builder.Gte(x => x.DateTime, ToUtc(query.From.Value));
            }

            if (query.To.HasValue)
            {
                filter &= builder.Lte(x => x.DateTime, ToUtc(query.To.Value));
            }

            if (query.Type.HasValue)
            {
                filter &= builder.Eq(x => x.Type, query.Type.Value);
            }

            if (!string.IsNullOrEmpty(query.BoardId))
            {
                filter &= builder.Eq(x => x.BoardId, query.BoardId);
            }

            if (!string.IsNullOrEmpty(query.Keyword))
            {
                filter &= builder.Regex(x => x.Title, new BsonRegularExpression(Regex.Escape(query.Keyword), "i"));
            }

            var limit = Math.Clamp(query.Limit <= 0 ? RssQuery.DefaultLimit : query.Limit, 1, RssQuery.MaxLimit);
            return await _collection.Find(filter)
                .SortByDescending(x => x.DateTime)
                .Limit(limit)
                .ToListAsync();
        }

        private const int MaxTitleLength = 100;

        private const int MaxDescriptionLength = 200;

        private static string Truncate(string text, int max)
        {
            text = Regex.Replace(text ?? string.Empty, @"\s+", " ").Trim();
            return text.Length <= max ? text : text[..(max - 1)].TrimEnd() + "…";
        }

        // 제목이 비어 있으면 카테고리 → 게시판/작성자 → 링크 순으로 대체
        private static string ResolveTitle(CrawlingData data, string board)
        {
            var title = Regex.Replace(data.Title ?? string.Empty, @"\s+", " ").Trim();
            if (!string.IsNullOrEmpty(title) && !title.Equals("No Title", StringComparison.OrdinalIgnoreCase))
            {
                return title;
            }

            if (!string.IsNullOrEmpty(data.Category))
            {
                return data.Category;
            }

            var source = $"{data.Type}/{board}";
            return string.IsNullOrEmpty(data.Author) ? $"{source} {data.Href}" : $"{source} - {data.Author}";
        }

        public static XDocument Build(IEnumerable<CrawlingData> datas, RssFlavor flavor, RssQuery query, string selfLink)
        {
            var channelTitle = flavor == RssFlavor.Short ? "web-crawler rss short" : "web-crawler rss";
            var description = string.Join(" ", new[]
            {
                query.From.HasValue ? $"from={query.From:yyyy-MM-dd HH:mm}" : null,
                query.To.HasValue ? $"to={query.To:yyyy-MM-dd HH:mm}" : null,
                query.Type.HasValue ? $"type={query.Type}" : null,
                string.IsNullOrEmpty(query.BoardId) ? null : $"boardId={query.BoardId}"
            }.Where(x => x != null));

            var channel = new XElement("channel",
                new XElement("title", channelTitle),
                new XElement("link", selfLink),
                new XElement("description", string.IsNullOrEmpty(description) ? channelTitle : description),
                new XElement("lastBuildDate", DateTimeOffset.Now.ToString("r", CultureInfo.InvariantCulture)));

            foreach (var data in datas)
            {
                var category = string.IsNullOrEmpty(data.Category) ? string.Empty : $"[{data.Category}] ";
                var board = string.IsNullOrEmpty(data.BoardName) ? data.BoardId : data.BoardName;

                var title = ResolveTitle(data, board);
                var item = new XElement("item",
                    new XElement("title", Truncate(flavor == RssFlavor.Short ? $"{category}{title}" : title, MaxTitleLength)),
                    new XElement("link", data.Href),
                    new XElement("guid", new XAttribute("isPermaLink", "true"), data.Href),
                    new XElement("pubDate", new DateTimeOffset(data.DateTime).ToString("r", CultureInfo.InvariantCulture)));

                if (!string.IsNullOrEmpty(data.Author))
                {
                    item.Add(new XElement("author", data.Author));
                }

                item.Add(new XElement("category", $"{data.Type}/{board}"));

                if (flavor == RssFlavor.Short)
                {
                    item.Add(new XElement("description", Truncate($"{data.Author} | {data.Type}/{board}", MaxDescriptionLength)));
                }
                else if (!string.IsNullOrEmpty(data.Category))
                {
                    item.Add(new XElement("description", Truncate(data.Category, MaxDescriptionLength)));
                }

                channel.Add(item);
            }

            return new XDocument(new XDeclaration("1.0", "utf-8", null),
                new XElement("rss", new XAttribute("version", "2.0"), channel));
        }
    }
}
