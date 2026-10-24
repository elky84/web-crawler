using AngleSharp.Dom;
using AngleSharp.Html.Parser;
using Serilog;
using System;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Server.Services
{
    // 크롤링된 글(href)의 본문 body 텍스트 앞부분을 미리보기로 가져온다. 실패하면 null 을 반환한다.
    public class ContentPreviewService
    {
        private const int PreviewLength = 100;

        private readonly IHttpClientFactory _httpClientFactory;

        public ContentPreviewService(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        // 호스트별 본문 영역 셀렉터. 매칭되는 사이트가 없거나 비어 있으면 og:description, body 순으로 폴백한다.
        private static readonly (string Host, string Selector)[] ContentSelectors =
        {
            ("dcinside.com", ".write_div"),
            ("ruliweb.com", ".view_content"),
            ("clien.net", ".post_article"),
            ("fmkorea.com", "article .xe_content"),
            ("ppomppu.co.kr", "td.board-contents"),
            ("todayhumor.co.kr", ".viewContent"),
            ("humoruniv.com", "#wrap_copy"),
            ("slrclub.com", "#userct"),
            ("inven.co.kr", "#powerbbsContent"),
        };

        private static string ExtractText(IDocument document, string href)
        {
            string host = Uri.TryCreate(href, UriKind.Absolute, out var uri) ? uri.Host : string.Empty;
            foreach (var (siteHost, selector) in ContentSelectors)
            {
                if (!host.EndsWith(siteHost, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var text = Normalize(document.QuerySelector(selector)?.TextContent);
                if (text.Length > 0)
                {
                    return text;
                }
            }

            var og = Normalize(document.QuerySelector("meta[property='og:description']")?.GetAttribute("content"));
            if (og.Length > 0)
            {
                return og;
            }

            return Normalize(document.Body?.TextContent);
        }

        private static string Normalize(string text)
            => Regex.Replace(Regex.Replace(text ?? string.Empty, @"\s+", " "), @"[\uD800-\uDBFF](?![\uDC00-\uDFFF])|(?<![\uD800-\uDBFF])[\uDC00-\uDFFF]", string.Empty).Trim();

        public async Task<string> GetPreviewAsync(string href)
        {
            if (string.IsNullOrEmpty(href))
            {
                return null;
            }

            try
            {
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
                var client = _httpClientFactory.CreateClient();
                client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/130.0.0.0 Safari/537.36");
                client.DefaultRequestHeaders.AcceptLanguage.ParseAdd("ko-KR,ko;q=0.9");

                using var response = await client.GetAsync(href, cts.Token);
                if (!response.IsSuccessStatusCode)
                {
                    Log.Warning("Preview fetch failed. <Href:{Href}> <StatusCode:{StatusCode}>", href, response.StatusCode);
                    return null;
                }

                var bytes = await response.Content.ReadAsByteArrayAsync(cts.Token);
                Encoding encoding;
                try
                {
                    var charset = response.Content.Headers.ContentType?.CharSet;
                    encoding = string.IsNullOrEmpty(charset) ? Encoding.GetEncoding("EUC-KR") : Encoding.GetEncoding(charset);
                }
                catch
                {
                    encoding = Encoding.UTF8;
                }

                var document = await new HtmlParser().ParseDocumentAsync(encoding.GetString(bytes), cts.Token);
                foreach (var node in document.QuerySelectorAll("script,style,noscript,nav,header,footer,iframe"))
                {
                    node.Remove();
                }

                var text = ExtractText(document, href);
                if (text.Length == 0)
                {
                    return null;
                }

                if (text.Length <= PreviewLength)
                {
                    return text;
                }

                // 서로게이트 페어(이모지, 수학 기호 등)가 중간에 잘리면 BSON UTF-8 직렬화가 실패하므로 앞쪽에서 끊는다.
                int length = char.IsHighSurrogate(text[PreviewLength - 1]) ? PreviewLength - 1 : PreviewLength;
                return text[..length] + "...";
            }
            catch (Exception e)
            {
                Log.Warning(e, "Preview failed. <Href:{Href}>", href);
                return null;
            }
        }
    }
}
