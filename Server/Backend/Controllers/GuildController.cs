using AndroidServer.Domain;
using RestApi;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.Controllers
{
    public class GuildController : RestController
    {
        private AndroidService Android => AndroidService.Instance;

        public override string Route => "guild";

        [Rest(HttpVerb.Get, "all")]
        public IEnumerable<GuildInfo> GetAll()
        {
            var infos = Android.GetGuildInfos();
            return infos;
        }

        [Rest(HttpVerb.Get, "")]
        public RestResponse Get(string id)
        {
            var info = Android.GetGuildInfos().FirstOrDefault(g => g.ID == id);

            if (info.ID == default)
                return RestResponse.BadRequest;

            return RestResponse.Ok;
        }

        [Rest(HttpVerb.Get, "channels")]
        public async Task<IEnumerable<ChannelInfo>> GetChannels(ulong guildID)
        {
            var infos = await Android.GetChannelsFrom(guildID);
            return infos;
        }

        [Rest(HttpVerb.Get, "roles")]
        public async Task<IEnumerable<RoleInfo>> GetRoles(ulong guildID)
        {
            var guild = await Android.Client.Rest.GetGuildAsync(guildID);
            var roles = guild.Roles;

            return roles.Select(r => new RoleInfo
            {
                Name = r.Name,
                ID = r.Id.ToString(),
                Color = $"rgb({r.Color.R},{r.Color.G},{r.Color.B})"
            });
        }

        [Rest(HttpVerb.Get, "user")]
        public async Task<UserInfo> GetUser(ulong id, ulong guildID)
        {
            var user = await Android.Client.Rest.GetGuildUserAsync(guildID, id);
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

        [Rest(HttpVerb.Get, "emotes")]
        public async Task<IEnumerable<EmoteInfo>> GetEmotes(ulong guildID)
        {
            var guild = await Android.Client.Rest.GetGuildAsync(guildID);
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
