using System;
using System.Collections.Generic;

namespace AndroidServer.Domain
{
    /// <summary>
    /// The serialisable state of an <see cref="AndroidInstance"/>
    /// </summary>
    [Serializable]
    public struct AndroidState
    {
        /// <summary>
        /// <see cref="AndroidInstance.Active"/>
        /// </summary>
        public bool Active { get; set; }
        /// <summary>
        /// <see cref="AndroidInstance.Listeners"/>
        /// </summary>
        public IEnumerable<AndroidListener> Listeners { get; set; }
        /// <summary>
        /// <see cref="AndroidModerationManager.MutesByUser"/>
        /// </summary>
        public Dictionary<ulong, MuteEntry> MutedUsers { get; set; }
    }
}
