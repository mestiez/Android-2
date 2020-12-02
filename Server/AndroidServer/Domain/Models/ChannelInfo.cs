using System;

namespace AndroidServer.Domain
{
    /// <summary>
    /// Serialisable information about a channel
    /// </summary>
    [Serializable]
    public class ChannelInfo
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
        /// Description
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// Text or Voice channel?
        /// </summary>
        public string ChannelType { get; set; }
        /// <summary>
        /// Position in the channel list
        /// </summary>
        public int Position { get; set; }
        /// <summary>
        /// ID of the parent category
        /// </summary>
        public string CategoryID { get; set; }
    }
}
