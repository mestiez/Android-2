using AndroidServer.Domain;
using Newtonsoft.Json;
using RestApi;
using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

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

        [Rest(HttpVerb.Post, "botStatus")]
        public async Task SetBotStatus()
        {
            var body = DeserialiseBody<StatusInfo>();

            Discord.ActivityType type = Discord.ActivityType.Playing;

            switch (body.Kind)
            {
                case "play":
                    type = Discord.ActivityType.Playing;
                    break;
                case "watch":
                    type = Discord.ActivityType.Watching;
                    break;
                case "listen":
                    type = Discord.ActivityType.Listening;
                    break;
            }

            await AndroidService.Instance.Client.SetGameAsync(body.Status, type: type);
        }
    }
}
