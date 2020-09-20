using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Server.Services;
using System.Threading.Tasks;

namespace Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class FeedController : ControllerBase
    {
        private readonly ILogger<FeedController> _logger;

        private readonly FeedCrawlingService _feedCrawlingService;

        public FeedController(ILogger<FeedController> logger, FeedCrawlingService feedCrawlingService)
        {
            _logger = logger;
            _feedCrawlingService = feedCrawlingService;
        }

        [HttpGet]
        public async Task<Protocols.Response.FeedList> Get([FromQuery] Protocols.Request.FeedList feedList)
        {
            return await _feedCrawlingService.Get(feedList);
        }

        [HttpPost]
        public async Task<Protocols.Response.Feed> Feed([FromBody] Protocols.Request.Feed feed)
        {
            return await _feedCrawlingService.Execute(feed);
        }
    }
}
