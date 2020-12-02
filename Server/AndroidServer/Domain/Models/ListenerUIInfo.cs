using System;
using System.Collections.Generic;

namespace AndroidServer.Domain
{
    /// <summary>
    /// Serialisable structure that holds all listener UI entries
    /// </summary>
    [Serializable]
    public class ListenerUIInfo
    {
        /// <summary>
        /// All UI entries
        /// </summary>
        public List<ListenerUIEntry> Variables { get; set; } = new ();
    }
}
