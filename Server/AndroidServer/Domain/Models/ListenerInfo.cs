using AndroidServer.Domain.Listeners;
using System;

namespace AndroidServer.Domain
{
    /// <summary>
    /// Serialisable structure that holds listener information
    /// </summary>
    [Serializable]
    public class ListenerInfo
    {
        /// <summary>
        /// <see cref="AndroidListener.Active"/>
        /// </summary>
        public bool Active { get; set; }

        /// <summary>
        /// <see cref="AndroidListener.Name"/>
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// <see cref="AndroidListener.DisplayName"/>
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// <see cref="AndroidListener.ID"/>
        /// </summary>
        public string ID { get; set; }

        /// <summary>
        /// <see cref="AndroidListener.GlobalListener"/>
        /// </summary>
        public bool GlobalListener { get; set; }

        /// <summary>
        /// Type info of the listener
        /// </summary>
        public ListenerType ListenerType { get; set; }

        /// <summary>
        /// <see cref="AndroidListener.ChannelID"/>
        /// </summary>
        public string ChannelID { get; set; }

        /// <summary>
        /// Construct a <see cref="ListenerInfo"/> from a listener
        /// </summary>
        public static ListenerInfo Create(AndroidListener listener)
        {
            return new ListenerInfo
            {
                Active = listener.Active,
                Name = listener.Name,
                DisplayName = listener.DisplayName,
                GlobalListener= listener.GlobalListener,
                ID = listener.ID,
                ChannelID = listener.ChannelID.ToString(),
                ListenerType = ListenerTypes.GetListenerType(listener.GetType()),
            };
        }
    }
}
