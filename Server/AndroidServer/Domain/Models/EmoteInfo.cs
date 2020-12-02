using System;

namespace AndroidServer.Domain
{
    /// <summary>
    /// Serialisable emote info
    /// </summary>
    [Serializable]
    public class EmoteInfo
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
        /// URL to the emote image
        /// </summary>
        public string URL { get; set; }
    }
}
