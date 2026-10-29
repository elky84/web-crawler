using AngleSharp.Dom;
using MongoDB.Driver;
using Serilog;
using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using WebCrawler.Models;

namespace WebCrawler.Crawler
{
    public partial class RuliwebCrawler(CrawlDataDelegate onCrawlDataDelegate, IMongoDatabase mongoDb, Source source)
        : CrawlerBase(onCrawlDataDelegate, mongoDb, $"https://bbs.ruliweb.com/{source.BoardId}", source)
    {
        protected override string UrlComposite(int page)
        {
            return $"{UrlBase}?page={page}";
        }

        // 행(tr) 단위로 셀 클래스(id, divsn, subject, writer, recomd, hit, time)를 직접 읽는다.
        // 제목/링크/작성자 등이 같은 행에서만 추출되므로 컬럼 수나 빈 셀이 달라져도 밀리지 않는다.
        protected override void OnPageCrawl(IDocument document)
        {
            var isBest = document.GetElementById("best_body") != null;

            var rows = document.QuerySelectorAll("tr.table_body")
                // 일반 게시판은 공지/광고 행(blocktarget 없음)을 제외, 베스트 목록은 전체 사용
                .Where(x => isBest || x.ClassList.Contains("blocktarget"))
                .Where(x => x.QuerySelector("td.subject a.subject_link") != null)
                .ToList();

            if (rows.Count == 0)
            {
                Log.Error("Parsing Failed DOM. Not has rows {UrlComposite}", UrlComposite(1));
                return;
            }

            foreach (var row in rows)
            {
                var link = row.QuerySelector("td.subject a.subject_link");
                var title = Clean(link.TextContent);
                var href = ToAbsoluteHref(link.GetAttribute("href"));
                if (string.IsNullOrEmpty(title) || string.IsNullOrEmpty(href))
                {
                    continue;
                }

                var id = ParseInt(Text(row, "td.id"));

                _ = OnCrawlData(new CrawlingData
                {
                    Type = Source.Type,
                    BoardId = Source.BoardId,
                    BoardName = Source.Name,
                    Category = Text(row, "td.divsn"),
                    Title = title,
                    Author = Text(row, "td.writer"),
                    Recommend = ParseInt(Text(row, "td.recomd")),
                    Count = ParseInt(Text(row, "td.hit")),
                    DateTime = ParseDate(Text(row, "td.time")),
                    RowId = id == 0 ? null : id,
                    Href = href,
                    SourceId = Source.Id
                });
            }
        }

        private static string Text(IElement row, string selector)
            => Clean(row.QuerySelector(selector)?.TextContent);

        private static string Clean(string text)
            => string.IsNullOrEmpty(text) ? string.Empty : WhiteSpaceRegex().Replace(text, " ").Trim();

        private static int ParseInt(string text)
        {
            var digits = DigitsRegex().Replace(text ?? string.Empty, string.Empty);
            return int.TryParse(digits, out var value) ? value : 0;
        }

        // 쿼리스트링(?cate=.., 끝의 ?)은 제거해서 일반/베스트 목록의 동일 글이 같은 Href로 중복 제거되게 한다.
        private string ToAbsoluteHref(string href)
        {
            if (string.IsNullOrWhiteSpace(href))
            {
                return string.Empty;
            }

            href = href.Split('?')[0];
            return href.StartsWith("http", StringComparison.OrdinalIgnoreCase) ? href : Host + href;
        }

        // "14:00" (오늘 시각), "26.10.29" / "2026.10.29" (날짜) 지원. 실패 시 현재 시각.
        private static DateTime ParseDate(string text)
        {
            text = text.Replace("날짜", string.Empty).Trim();

            var culture = (CultureInfo)Thread.CurrentThread.CurrentCulture.Clone();
            culture.Calendar.TwoDigitYearMax = DateTime.Now.Year + 30;

            return DateTime.TryParseExact(text, ["yyyy.MM.dd", "yy.MM.dd", "H:mm", "HH:mm"], culture, DateTimeStyles.None, out var date)
                ? date
                : DateTime.Now;
        }

        [GeneratedRegex(@"\s+")]
        private static partial Regex WhiteSpaceRegex();

        [GeneratedRegex(@"[^\d]")]
        private static partial Regex DigitsRegex();
    }
}
