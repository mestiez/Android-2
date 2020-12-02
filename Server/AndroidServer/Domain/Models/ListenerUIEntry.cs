using System;

namespace AndroidServer.Domain
{
    /// <summary>
    /// Serialisable structure that holds information specific to a single exposed listener property
    /// </summary>
    [Serializable]
    public class ListenerUIEntry
    {
        /// <summary>
        /// Identifying property name
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Type of the property
        /// </summary>
        public VariableType Type { get; set; }
    }
}
