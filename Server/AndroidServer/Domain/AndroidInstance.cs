using Discord;
using Discord.WebSocket;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AndroidServer.Domain
{
    /// <summary>
    /// An instance of Android that lives in a guild
    /// </summary>
    public class AndroidInstance
    {
        /// <summary>
        /// Active state of the entire instance
        /// </summary>
        public bool Active { get; set; }
        /// <summary>
        /// ID of the guild this instance is connected to
        /// </summary>
        public ulong GuildID { get; }
        /// <summary>
        /// The Discord client that this instance uses. This is actually a static value but is left an instance member in case this changes in the future
        /// </summary>
        [JsonIgnore]
        public DiscordSocketClient Client => AndroidService.Instance.Client;
        /// <summary>
        /// The raw list of listeners that live in this instance. Generally should not be edited directly but is left accesible in case you need to edit it anyway
        /// </summary>
        public List<AndroidListener> Listeners { get; } = new List<AndroidListener>();
        /// <summary>
        /// The @everyone role for the guild
        /// </summary>
        [JsonIgnore]
        public IRole EveryoneRole { get; }
        /// <summary>
        /// The guild
        /// </summary>
        [JsonIgnore]
        public IGuild Guild { get; }
        /// <summary>
        /// The moderation manager. <b>This won't function correctly unless configured via the client using a <see cref="Domain.Listeners.Commands.ModerationCommandListener"/></b>
        /// </summary>
        [JsonIgnore]
        public AndroidModerationManager Moderation { get; }

        private readonly Dictionary<ulong, Question> waitingForReply = new();
        private readonly TimeSpan waitingExpiration = TimeSpan.FromMinutes(1);
        private readonly Timer timer;

        /// <summary>
        /// Construct an instance
        /// </summary>
        public AndroidInstance(ulong guildID)
        {
            GuildID = guildID;

            Guild = Client.GetGuild(guildID);
            EveryoneRole = Guild.EveryoneRole;

            Client.MessageReceived += OnMessageReceived;

            Moderation = new AndroidModerationManager(this);

            Moderation.Initialise();

            timer = new Timer(async (ob) =>
            {
                await InvokeHourlyTick();
            }, null, 2000, (int)TimeSpan.FromHours(1).TotalMilliseconds);
        }

        /// <summary>
        /// Add a new listener of the given type to the given channel
        /// </summary>
        public void AddListener(Type type, ulong channel)
        {
            var instance = Activator.CreateInstance(type, this, channel) as AndroidListener;
            Listeners.Add(instance);
            instance.Initialise();
        }

        /// <summary>
        /// Remove a specific listener. <b>This does NOT call <see cref="AndroidListener.OnDelete"/></b>
        /// </summary>
        public bool RemoveListener(AndroidListener listener)
        {
            return Listeners.Remove(listener);
        }

        /// <summary>
        /// Get listeners by generic type parameter
        /// </summary>
        public IEnumerable<T> GetListeners<T>() where T : AndroidListener
        {
            foreach (var item in Listeners)
                if (item is T typed)
                    yield return typed;
        }

        /// <summary>
        /// Get a listener by generic type parameter
        /// </summary>
        public T GetListener<T>() where T : AndroidListener
        {
            foreach (var item in Listeners)
                if (item is T typed)
                    return typed;
            return default;
        }

        /// <summary>
        /// Tell all listeners to shut down
        /// </summary>
        public void Shutdown()
        {
            foreach (var item in Listeners)
                item.OnShutdown();

            Moderation.Shutdown();
            Listeners.Clear();
            Client.MessageReceived -= OnMessageReceived;
        }

        private async Task InvokeHourlyTick()
        {
            Console.WriteLine("Hourly tick");
            foreach (var listener in Listeners)
            {
                await listener.EveryHour();
            }
        }

        private async Task OnMessageReceived(SocketMessage message)
        {
            if (!Active) return;
            if (message.Author.IsBot) return;

            if (waitingForReply.TryGetValue(message.Author.Id, out var q))
            {
                if ((DateTime.UtcNow - q.AskTime) < waitingExpiration)
                    _ = Task.Run(() => q.ResponseFunction?.Invoke(message));

                waitingForReply.Remove(message.Author.Id);
            }

            ActIfPassing(message, async (listener) =>
            {
                await listener.OnMessage(message);
            });

            await Task.CompletedTask;
        }

        private void ActIfPassing(IMessage message, Func<AndroidListener, Task> action)
        {
            if (message == null) return;
            if (action == null) return;
            if (message.Source != MessageSource.User) return;
            if (message.Author.IsBot) return;

            var channel = message.Channel as SocketGuildChannel;
            if (channel == null || channel.Guild.Id != Guild.Id) return;

            var user = channel.GetUser(message.Author.Id);

            if (user == null) return;

            foreach (var listener in Listeners)
            {
                if (!listener.Active) continue;

                bool allowedChannel = listener.GlobalListener || listener.ChannelID == channel.Id;
                bool allowedUser = listener.ResponseFilters.PassesFilter(user, message);

                if (allowedChannel && allowedUser)
                    try
                    {
                        _ = Task.Run(() => action(listener));
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Error when calling listener {0}: {1}", listener.Name, e.Message);
                    }
            }
        }

        /// <summary>
        /// Make this instance ask a user a question. The given delegate will be invoked once the user responds
        /// </summary>
        /// <param name="userID">ID of the user to ask something</param>
        /// <param name="channel">Channel in which to ask it</param>
        /// <param name="function">Function to invoke when the user replies</param>
        /// <param name="message">Actual question text to ask, can be null</param>
        public async Task Ask(ulong userID, IMessageChannel channel, Func<SocketMessage, Task> function, string message = null)
        {
            if (!string.IsNullOrWhiteSpace(message))
                await channel.SendMessageAsync(message);

            waitingForReply.Add(userID, new Question
            {
                AskTime = DateTime.UtcNow,
                ResponseFunction = function
            });
        }

        private struct Question
        {
            public DateTime AskTime { get; set; }
            public Func<SocketMessage, Task> ResponseFunction { get; set; }
        }
    }
}
