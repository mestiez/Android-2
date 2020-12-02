using AndroidServer.Domain;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AndroidServer.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class GuildController : ControllerBase
    {
        private readonly AndroidService android;

        public GuildController(AndroidService android)
        {
            this.android = android;
        }

        [HttpGet("all")]
        public IEnumerable<GuildInfo> GetAll()
        {
            var infos = android.GetGuildInfos();
            return infos;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult Get(string id)
        {
            var info = android.GetGuildInfos().FirstOrDefault(g => g.ID == id);

            if (info.ID == default)
                return NotFound();

            return Ok(info);
        }

        [HttpGet("channels")]
        public async Task<IEnumerable<ChannelInfo>> GetChannels(ulong guildID)
        {
            var infos = await android.GetChannelsFrom(guildID);
            return infos;
        }

        [HttpGet("roles")]
        public async Task<IEnumerable<RoleInfo>> GetRoles(ulong guildID)
        {
            var guild = await android.Client.Rest.GetGuildAsync(guildID);
            var roles = guild.Roles;

            return roles.Select(r => new RoleInfo
            {
                Name = r.Name,
                ID = r.Id.ToString(),
                Color = $"rgb({r.Color.R},{r.Color.G},{r.Color.B})"
            });
        }

        [HttpGet("user")]
        public async Task<UserInfo> GetUser(ulong id, ulong guildID)
        {
            var user = await android.Client.Rest.GetGuildUserAsync(guildID, id);
            if (user == null)
                return null;

            return new UserInfo
            {
                ID = user.Id.ToString(),
                Discriminator = user.Discriminator,
                Nickname = user.Nickname ?? user.Username,
                Username = user.Username
            };
        }

        [HttpGet("emotes")]
        public async Task<IEnumerable<EmoteInfo>> GetEmotes(ulong guildID)
        {
            var guild = await android.Client.Rest.GetGuildAsync(guildID);
            var emotes = guild.Emotes;

            return emotes.Select(e => new EmoteInfo
            {
                ID = e.Id.ToString(),
                Name = e.Name,
                URL = e.Url
            });
        }
    }
}
