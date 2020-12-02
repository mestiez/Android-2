using Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AndroidServer.Domain
{
    /// <summary>
    /// Structure that holds a black-/whitelist
    /// </summary>
    [Serializable]
    public class ResponseFilters
    {
        /// <summary>
        /// Should the filter be treated as a blacklist? It will be treated as a whitelist if this value is set to false. True by default
        /// </summary>
        public bool Blacklist { get; set; } = true;

        /// <summary>
        /// Users in the black-/whitelist
        /// </summary>
        public List<UserInfo> Users { get; set; } = new();

        /// <summary>
        /// Roles in the black-/whitelist
        /// </summary>
        public List<RoleInfo> Roles { get; set; } = new();

        /// <summary>
        /// Does the given user and message pass the filter?
        /// </summary>
        public bool PassesFilter(IGuildUser guildUser, IMessage message)
        {
            if (Blacklist && Users.Count == 0 && Roles.Count == 0)
                return true;

            foreach (var item in Users)
                if (item.ID == message.Author.Id.ToString())
                    return !Blacklist;

            //TODO is dit betrouwbaar? 
            var user = guildUser;// await android.Guild.GetUserAsync(message.Author.Id);
            //var user = await android.Client.Rest.GetGuildUserAsync(android.GuildID, message.Author.Id);

            foreach (var a in Roles)
                foreach (var b in user.RoleIds)
                    if (a.ID == b.ToString())
                        return !Blacklist;

            return Blacklist;
        }
    }
}
