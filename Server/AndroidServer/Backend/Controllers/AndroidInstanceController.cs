using AndroidServer.Domain;
using Discord;
using Newtonsoft.Json;
using RestApi;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Backend.Controllers
{
    public class AndroidInstanceController : RestController
    {
        private AndroidService Android => AndroidService.Instance;

        public override string Route => "instance";

        [Rest(HttpVerb.Get, "listeners")]
        public IEnumerable<ListenerInfo> GetListeners(ulong guildID, ulong channelID)
        {
            if (!Android.AndroidInstances.TryGetValue(guildID, out var instance))
                return Array.Empty<ListenerInfo>();

            return instance.Listeners.Where(l => l.ChannelID == channelID || l.GlobalListener).OrderBy(l => l.GlobalListener).Select(l => ListenerInfo.Create(l));
        }

        [Rest(HttpVerb.Post, "listeners")]
        public RestResponse AddListener(ulong guildID, ulong channelID, string listenerTypeID)
        {
            if (!Android.AndroidInstances.TryGetValue(guildID, out var instance))
                return new RestResponse(HttpStatusCode.BadRequest, "Invalid guild ID");
            var listenerType = ListenerTypes.GetRawType(listenerTypeID);
            if (listenerType == null)
                return new RestResponse(HttpStatusCode.BadRequest, "Invalid listener type ID");

            instance.AddListener(listenerType, channelID);
            return RestResponse.Ok;
        }

        [Rest(HttpVerb.Post, "active")]
        public void SetActive(ulong guildID, bool active)
        {
            if (!Android.AndroidInstances.TryGetValue(guildID, out var instance))
                return;

            instance.Active = active;
        }

        [Rest(HttpVerb.Get, "active")]
        public bool GetActive(ulong guildID)
        {
            if (!Android.AndroidInstances.TryGetValue(guildID, out var instance))
                return false;

            return instance.Active;
        }

        [Rest(HttpVerb.Delete, "listeners")]
        public void RemoveListener(ulong guildID, string listenerID)
        {
            if (!Android.AndroidInstances.TryGetValue(guildID, out var instance)) return;
            var listener = Android.FindListener(listenerID);
            if (listener == null) return;

            listener.OnDelete();
            instance.RemoveListener(listener);
        }

        [Rest(HttpVerb.Post, "say")]
        public async Task<RestResponse> Say(ulong channelID)
        {
            var parts = CurrentBody.Split('␞');

            if (parts.Length != 3)
                return new RestResponse(HttpStatusCode.BadRequest, $"Expected 3 lines in body, got {parts.Length}");

            int c = CurrentBody.Length;

            var content = parts[0];
            var fileName = parts[1];
            var fileB64 = parts[2];

            var channel = await Android.Client.Rest.GetChannelAsync(channelID);
            if (channel is ITextChannel tc)
            {
                if (!string.IsNullOrWhiteSpace(fileB64))
                {
                    var bytes = Convert.FromBase64String(fileB64);
                    using MemoryStream stream = new MemoryStream(bytes);
                    await tc.SendFileAsync(stream, fileName, content);
                }
                else
                {
                    await tc.SendMessageAsync(content);
                }
            }

            return RestResponse.Ok;
        }

        [Rest(HttpVerb.Delete, "")]
        public async Task DeleteInstance(ulong guildID)
        {
            if (!Android.AndroidInstances.Remove(guildID, out var instance))
                return;

            instance.Shutdown();

            var guild = Android.Client.GetGuild(guildID);
            await guild.LeaveAsync();
        }

        [Rest(HttpVerb.Post, "save")]
        public void SaveToDisk(ulong guildID)
        {
            if (!Android.AndroidInstances.TryGetValue(guildID, out var instance))
                return;

            AndroidStateSerialiser.Save(instance);
        }
    }
}
