using System;

namespace AndroidServer.Domain
{
    /// <summary>
    /// Serialisable role information
    /// </summary>
    [Serializable]
    public class RoleInfo
    {
        /// <summary>
        /// Role ID
        /// </summary>
        public string ID { get; set; }
        /// <summary>
        /// Role name
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Role colour
        /// </summary>
        public string Color { get; set; }
    }
}
