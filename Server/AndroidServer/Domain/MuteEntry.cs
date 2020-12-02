using System;

namespace AndroidServer.Domain
{
    /// <summary>
    /// An entry in the mute list
    /// </summary>
    public struct MuteEntry
    {
        /// <summary>
        /// Construct a mute entry
        /// </summary>
        public MuteEntry(ulong userId, ulong channelId, ulong guildId, DateTime expiration) : this()
        {
            UserID = userId;
            ChannelID = channelId;
            GuildID = guildId;
            Expiration = expiration;
        }

        /// <summary>
        /// Muted user ID
        /// </summary>
        public ulong UserID { get; set; }
        /// <summary>
        /// ID of the channel where the user was muted
        /// </summary>
        public ulong ChannelID { get; set; }
        /// <summary>
        /// ID of the guild in which the mute took place
        /// </summary>
        public ulong GuildID { get; set; }
        /// <summary>
        /// When the mute will end
        /// </summary>
        public DateTime Expiration { get; set; }
    }
}
