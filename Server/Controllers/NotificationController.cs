using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Server.Models;
using Server.Services;
using System.Threading.Tasks;

namespace Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class NotificationController : ControllerBase
    {
        private readonly ILogger<NotificationController> _logger;

        private readonly NotificationService _notificationService;

        public NotificationController(ILogger<NotificationController> logger, NotificationService sourceService)
        {
            _logger = logger;
            _notificationService = sourceService;
        }


        [HttpGet]
        public async Task<Protocols.Response.NotificationMulti> All()
        {
            return new Protocols.Response.NotificationMulti
            {
                NotificationDatas = (await _notificationService.All()).ConvertAll(x => x.ToProtocol())
            };
        }

        [HttpPost]
        public async Task<Protocols.Response.Notification> Create([FromBody] Protocols.Request.Notification notificaion)
        {
            return await _notificationService.Create(notificaion);
        }

        [HttpPost("Multi")]
        public async Task<Protocols.Response.NotificationMulti> CreateMulti([FromBody] Protocols.Request.NotificationMulti notificaionMulti)
        {
            return await _notificationService.CreateMulti(notificaionMulti);
        }


        [HttpGet("{id}")]
        public async Task<Protocols.Response.Notification> Get(string id)
        {
            return await _notificationService.Get(id);
        }

        [HttpPut("{id}")]
        public async Task<Protocols.Response.Notification> Update(string id, [FromBody] Protocols.Request.NotificationUpdate notificaion)
        {
            return await _notificationService.Update(id, notificaion);
        }

        [HttpDelete("{id}")]
        public async Task<Protocols.Response.Notification> Delete(string id)
        {
            return await _notificationService.Delete(id);
        }
    }
}
