using System;

namespace AndroidServer.Domain
{
    /// <summary>
    /// Serialisable guild info
    /// </summary>
    [Serializable]
    public class GuildInfo
    {
        /// <summary>
        /// ID
        /// </summary>
        public string ID { get; set; }
        /// <summary>
        /// Name
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// URL to the guild icon
        /// </summary>
        public string IconURL { get; set; }
    }
}
