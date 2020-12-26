using RestApi;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Backend
{
    public class TokenFilter : RequestFilter
    {
        public const string SecretHeader = "auth";
        public List<string> GeneratedSecrets { get; set; } = new List<string>();

        public override bool IsAllowed(HttpListenerContext context)
        {
            var route = RestServer.GetRouteFromRequest(context.Request);

            if (RestServer.MatchingRoute(route, "system/authorise"))
                return true;

            var headerContent = context.Request.Headers.Get(SecretHeader);
            if (headerContent == null || !GeneratedSecrets.Contains(headerContent))
            {
                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                context.Response.StatusDescription = "Unauthorised";
                context.Response.Close();
                return false;
            }

            return true;
        }
    }
}
