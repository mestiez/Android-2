using Newtonsoft.Json;
using System.Net;

namespace RestApi
{
    public abstract class RestController
    {
        public RestServer Server { get; internal set; }
        public HttpListenerContext CurrentContext { get; internal set; }
        public string CurrentBody { get; internal set; }

        protected T DeserialiseBody<T>()
        {
            return JsonConvert.DeserializeObject<T>(CurrentBody, Server.SerializerSettings);
        }

        public abstract string Route { get; }
    }
}
