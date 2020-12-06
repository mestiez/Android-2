using System.Net;

namespace RestApi
{
    public class RestResponse
    {
        public object ReponseBody;
        public string StatusDescription;
        public HttpStatusCode StatusCode = HttpStatusCode.OK;

        public RestResponse(HttpStatusCode code)
        {
            StatusCode = code;
        }

        public RestResponse(HttpStatusCode code, string message)
        {
            StatusCode = code;
            StatusDescription = message;
        }

        public RestResponse(HttpStatusCode code, string message, object body)
        {
            StatusCode = code;
            StatusDescription = message;
            ReponseBody = body;
        }

        public static RestResponse Ok => new RestResponse(HttpStatusCode.OK);
        public static RestResponse BadRequest => new RestResponse(HttpStatusCode.BadRequest);
        public static RestResponse NotFound => new RestResponse(HttpStatusCode.NotFound);
    }
}
