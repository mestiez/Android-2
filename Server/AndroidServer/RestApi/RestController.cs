using System.Net;

namespace RestApi
{
    public abstract class RestController
    {
        public RestServer Server { get; internal set; }
        public HttpListenerContext CurrentContext { get; internal set; }
        public string CurrentBody { get; internal set; }

        public abstract string Route { get; }
    }
}
