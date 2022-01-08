using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Linq;
using WebCrawler.Code;
using MongoDbWebUtil.Util;

namespace Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CodeController : ControllerBase
    {
        private readonly ILogger<CodeController> _logger;

        public CodeController(ILogger<CodeController> logger)
        {
            _logger = logger;
        }

        [HttpGet("CrawlingType")]
        public Protocols.Response.CodeList Get()
        {
            return new Protocols.Response.CodeList
            {
                Datas = TypesUtil.ToEnumerable<CrawlingType>()
                                 .Where(x => x != CrawlingType.Rss)
                                 .ToList()
                                 .ConvertAll(x => x.ToString())
            };
        }
    }
}
