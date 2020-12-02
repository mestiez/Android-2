using AndroidServer.Domain;
using AndroidServer.Domain.Listeners;
using Discord;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace AndroidServer.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AndroidInstanceController : Controller
    {
        private readonly AndroidService android;

        public AndroidInstanceController(AndroidService android)
        {
            this.android = android;
        }

        [HttpGet("listeners")]
        public IEnumerable<ListenerInfo> GetListeners(ulong guildID, ulong channelID)
        {
            if (!android.AndroidInstances.TryGetValue(guildID, out var instance))
                return Array.Empty<ListenerInfo>();

            return instance.Listeners.Where(l => l.ChannelID == channelID || l.GlobalListener).OrderBy(l => l.GlobalListener).Select(l => ListenerInfo.Create(l));
        }

        [HttpPost("listeners")]
        public IActionResult AddListener(ulong guildID, ulong channelID, string listenerTypeID)
        {
            if (!android.AndroidInstances.TryGetValue(guildID, out var instance))
                return BadRequest("Invalid guild ID");
            var listenerType = ListenerTypes.GetRawType(listenerTypeID);
            if (listenerType == null)
                return BadRequest("Invalid listener type ID");

            instance.AddListener(listenerType, channelID);
            return Ok();
        }

        [HttpPost("active")]
        public void SetActive(ulong guildID, bool active)
        {
            if (!android.AndroidInstances.TryGetValue(guildID, out var instance))
                return;

            instance.Active = active;
        }

        [HttpGet("active")]
        public bool GetActive(ulong guildID)
        {
            if (!android.AndroidInstances.TryGetValue(guildID, out var instance))
                return false;

            return instance.Active;
        }

        [HttpDelete("listeners")]
        public void RemoveListener(ulong guildID, string listenerID)
        {
            if (!android.AndroidInstances.TryGetValue(guildID, out var instance)) return;
            var listener = android.FindListener(listenerID);
            if (listener == null) return;

            listener.OnDelete();
            instance.RemoveListener(listener);
        }

        [HttpPost("say")]
        public async Task Say(ulong channelID, [FromBody] SayInfo message)
        {
            var channel = await android.Client.Rest.GetChannelAsync(channelID);
            if (channel is ITextChannel tc)
            {
                if (!string.IsNullOrWhiteSpace(message.FileB64))
                {
                    var bytes = Convert.FromBase64String(message.FileB64);
                    MemoryStream stream = new MemoryStream(bytes);
                    await tc.SendFileAsync(stream, message.FileName, message.Message);
                    await stream.DisposeAsync();
                }
                else
                {
                    await tc.SendMessageAsync(message.Message);
                }
            }

            await Task.CompletedTask;
        }

        [HttpDelete()]
        public async Task DeleteInstance(ulong guildID)
        {
            if (!android.AndroidInstances.Remove(guildID, out var instance))
                return;

            instance.Shutdown();

            var guild = android.Client.GetGuild(guildID);
            await guild.LeaveAsync();
        }

        [HttpPost("save")]
        public void SaveToDisk(ulong guildID)
        {
            if (!android.AndroidInstances.TryGetValue(guildID, out var instance))
                return;

            AndroidStateSerialiser.Save(instance);
        }
    }
}
