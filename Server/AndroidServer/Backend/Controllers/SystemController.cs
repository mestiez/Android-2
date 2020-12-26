using AndroidServer.Domain;
using Newtonsoft.Json;
using RestApi;
using System;
using System.Security.Cryptography;
using System.Text;

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

        [Rest(HttpVerb.Get, "authorise")]
        public string Authorise(string botToken)
        {
            //The given token is expected to be a UTF8 string encoded in base64
            string realToken64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(AndroidService.Instance.BotToken));
            var valid = realToken64 == botToken;

            if (valid)
            {
                var secret = Guid.NewGuid().ToString();
                Server.GetFilter<TokenFilter>().GeneratedSecrets.Add(secret);
                return JsonConvert.SerializeObject(secret);
            }

            return null;
        }
    }
}
