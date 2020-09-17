using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Server.Models;
using Server.Services;
using System.Threading.Tasks;

namespace Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SourceController : ControllerBase
    {
        private readonly ILogger<SourceController> _logger;

        private readonly SourceService _sourceService;

        public SourceController(ILogger<SourceController> logger, SourceService sourceService)
        {
            _logger = logger;
            _sourceService = sourceService;
        }



        [HttpGet]
        public async Task<Protocols.Response.SourceMulti> All()
        {
            return new Protocols.Response.SourceMulti
            {
                SourceDatas = (await _sourceService.All()).ConvertAll(x => x.ToProtocol())
            };
        }

        [HttpPost]
        public async Task<Protocols.Response.Source> Create([FromBody] Protocols.Request.Source source)
        {
            return await _sourceService.Create(source);
        }

        [HttpPost("Multi")]
        public async Task<Protocols.Response.SourceMulti> CreateMulti([FromBody] Protocols.Request.SourceMulti sourceMulti)
        {
            return await _sourceService.CreateMulti(sourceMulti);
        }


        [HttpGet("{id}")]
        public async Task<Protocols.Response.Source> Get(string id)
        {
            return await _sourceService.GetById(id);
        }

        [HttpPut("{id}")]
        public async Task<Protocols.Response.Source> Update(string id, [FromBody] Protocols.Request.Source source)
        {
            return await _sourceService.Update(id, source);
        }

        [HttpDelete("{id}")]
        public async Task<Protocols.Response.Source> Delete(string id)
        {
            return await _sourceService.Delete(id);
        }
    }
}
