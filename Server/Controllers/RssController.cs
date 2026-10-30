using Microsoft.AspNetCore.Mvc;
using Server.Services;
using System.Text;
using System.Threading.Tasks;

namespace Server.Controllers
{
    [ApiController]
    public class RssController(RssService rssService) : ControllerBase
    {
        // 기간 내 크롤링 데이터를 rss.xml 로 응답 (download=true 면 파일로 내려줌)
        [HttpGet("rss.xml")]
        public Task<IActionResult> Rss([FromQuery] RssQuery query, [FromQuery] bool download = false)
            => Render(query, RssFlavor.Rss, download);

        // 짧은 형태([카테고리] 제목 / 작성자)로 rss.xml 응답. type(사이트 종류), boardId 조건 지원
        [HttpGet("short.xml")]
        public Task<IActionResult> Short([FromQuery] RssQuery query, [FromQuery] bool download = false)
            => Render(query, RssFlavor.Short, download);

        private async Task<IActionResult> Render(RssQuery query, RssFlavor flavor, bool download)
        {
            var datas = await rssService.Find(query);
            var xml = RssService.Build(datas, flavor, query, $"{Request.Scheme}://{Request.Host}{Request.Path}").ToString();
            var bytes = new UTF8Encoding(false).GetBytes(xml);
            const string contentType = "application/rss+xml; charset=utf-8";
            return download ? File(bytes, contentType, "rss.xml") : File(bytes, contentType);
        }
    }
}
