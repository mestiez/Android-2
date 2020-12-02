using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;

namespace AndroidServer.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class SystemController : ControllerBase
    {
        public IHostApplicationLifetime Lifetime { get; }

        public SystemController(IHostApplicationLifetime lifeTime)
        {
            Lifetime = lifeTime;
        }

        [HttpPost("shutdown")]
        public void Shutdown()
        {
            Lifetime.StopApplication();
        }
    }
}
