using System;

namespace AndroidServer.Domain
{
    /// <summary>
    /// Serialisable information about a guild user
    /// </summary>
    [Serializable]
    public class UserInfo
    {
        /// <summary>
        /// ID
        /// </summary>
        public string ID { get; set; }
        /// <summary>
        /// Username
        /// </summary>
        public string Username { get; set; }
        /// <summary>
        /// Discriminator value
        /// </summary>
        public string Discriminator { get; set; }
        /// <summary>
        /// Guild nickname
        /// </summary>
        public string Nickname { get; set; }
    }
}
