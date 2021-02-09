using Discord;
using Discord.WebSocket;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace AndroidServer.Domain
{
    /// <summary>
    /// Base class for any listener
    /// </summary>
    public abstract class AndroidListener
    {
        private bool active = false;

        /// <summary>
        /// Listener active state
        /// </summary>
        public bool Active
        {
            get => active; set
            {
                if (value && !active)
                    OnEnable();

                if (!value && active)
                    OnDisable();

                active = value;
            }
        }

        /// <summary>
        /// Reference to the parent instance
        /// </summary>
        [JsonIgnore]
        public AndroidInstance Android { get; set; }
        /// <summary>
        /// ID of the channel that this listener lives in
        /// </summary>
        public ulong ChannelID { get; }

        /// <summary>
        /// Display name in the client interface. Can be null
        /// </summary>
        [UiVariableType(VariableType.String)]
        public string DisplayName { get; set; }

        /// <summary>
        /// Listener global state. This will make the listener respond to events in all channels
        /// </summary>
        [UiVariableType(VariableType.Boolean)]
        public bool GlobalListener { get; set; }

        /// <summary>
        /// Unique ID of this listener
        /// </summary>
        public string ID { get; }

        /// <summary>
        /// Response filters for the listener. Stores black-/whitelist
        /// </summary>
        public ResponseFilters ResponseFilters { get; set; } = new ResponseFilters { Blacklist = true };

        /// <summary>
        /// Construct a listener
        /// </summary>
        protected AndroidListener(AndroidInstance android, ulong channelID)
        {
            Android = android;
            ChannelID = channelID;
            ID = IdentityGen.Generate();
            OnEnable();

            SetDisplayNameDefault();
        }

        /// <summary>
        /// Construct a listener
        /// </summary>
        protected AndroidListener(AndroidInstance android)
        {
            Android = android;
            ChannelID = 0;
            GlobalListener = true;
            ID = IdentityGen.Generate();
            OnEnable();

            SetDisplayNameDefault();
        }

        private void SetDisplayNameDefault()
        {
            if (string.IsNullOrWhiteSpace(DisplayName))
                DisplayName = Name;
        }

        /// <summary>
        /// Default name of the listener and also the read-only name of the listener
        /// </summary>
        public virtual string Name => GetType().Name;

        /// <summary>
        /// Called when a message is sent to a channel this listener is listening to. Base class does nothing here
        /// </summary>
        public virtual Task OnMessage(SocketMessage message) { return Task.CompletedTask; }

        /// <summary>
        /// Called when the Android instance is being initialised
        /// </summary>
        public virtual void Initialise() { }
        /// <summary>
        /// Called when this listener is deleted
        /// </summary>
        public virtual void OnDelete() { }
        /// <summary>
        /// Called when the server is being shut down
        /// </summary>
        public virtual void OnShutdown() { }
        /// <summary>
        /// Called when this listener is enabled
        /// </summary>
        protected virtual void OnEnable() { }
        /// <summary>
        /// Called when this listener is disabled
        protected virtual void OnDisable() { }
        /// <summary>
        /// Called every hour for every listener
        /// </summary>
        public virtual Task EveryHour() { return Task.CompletedTask; }
        /// <summary>
        /// Called when a property of this listener is changed by the client
        /// </summary>
        public virtual void OnPropertyChange(string name, object value) { }
    }
}
