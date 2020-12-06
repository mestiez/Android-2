using System;

namespace RestApi
{
    [AttributeUsage(AttributeTargets.Method)]
    public class RestAttribute : Attribute
    {
        public HttpVerb Verb = HttpVerb.Get;
        public string Route;

        public RestAttribute(HttpVerb verb, string route)
        {
            Verb = verb;
            Route = route;
        }
    }
}
