using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Server.Services;
using System.Threading.Tasks;

namespace Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CrawlingController : ControllerBase
    {
        private readonly ILogger<CrawlingController> _logger;

        private readonly CrawlingService _crawlingService;

        public CrawlingController(ILogger<CrawlingController> logger, CrawlingService crawlingService)
        {
            _logger = logger;
            _crawlingService = crawlingService;
        }

        [HttpPost]
        public async Task<Protocols.Response.Crawling> Crawling([FromBody] Protocols.Request.Crawling crawling)
        {
            return await _crawlingService.Execute(crawling);
        }
    }
}
