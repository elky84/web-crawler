using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Server.Models;
using Server.Services;
using System.Threading.Tasks;

namespace Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class RssController : ControllerBase
    {
        private readonly ILogger<RssController> _logger;

        private readonly RssService _rssService;

        public RssController(ILogger<RssController> logger, RssService rssService)
        {
            _logger = logger;
            _rssService = rssService;
        }



        [HttpGet]
        public async Task<Protocols.Response.RssMulti> All()
        {
            return new Protocols.Response.RssMulti
            {
                Datas = (await _rssService.All()).ConvertAll(x => x.ToProtocol())
            };
        }

        [HttpPost]
        public async Task<Protocols.Response.Rss> Create([FromBody] Protocols.Request.Rss rss)
        {
            return await _rssService.Create(rss);
        }

        [HttpPost("Multi")]
        public async Task<Protocols.Response.RssMulti> CreateMulti([FromBody] Protocols.Request.RssMulti rssMulti)
        {
            return await _rssService.CreateMulti(rssMulti);
        }


        [HttpGet("{id}")]
        public async Task<Protocols.Response.Rss> Get(string id)
        {
            return await _rssService.GetById(id);
        }

        [HttpPut("{id}")]
        public async Task<Protocols.Response.Rss> Update(string id, [FromBody] Protocols.Request.Rss rss)
        {
            return await _rssService.Update(id, rss);
        }

        [HttpDelete("{id}")]
        public async Task<Protocols.Response.Rss> Delete(string id)
        {
            return await _rssService.Delete(id);
        }
    }
}
