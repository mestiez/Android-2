using System;

namespace AndroidServer.Domain
{
    /// <summary>
    /// Serialisable structure that holds primitive identifying information about a listener type
    /// </summary>
    [Serializable]
    public class ListenerType
    {
        /// <summary>
        /// Type name
        /// </summary>
        public string TypeName { get; set; }
        /// <summary>
        /// Type ID
        /// </summary>
        public string TypeID { get; set; }
    }
}
