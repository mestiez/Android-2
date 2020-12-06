using AndroidServer.Domain;
using RestApi;

namespace Backend.Controllers
{
    public class SystemController : RestController
    {
        public override string Route => "system";

        [Rest(HttpVerb.Post, "shutdown")]
        public void Shutdown()
        {
            AndroidService.Instance.Stop();
            Server.Stop();
        }
    }
}
