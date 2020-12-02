using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace AndroidServer.Domain
{
    /// <summary>
    /// The global bot service
    /// </summary>
    public class AndroidService
    {
        private const string BotTokenFilePath = "bot_token";

        /// <summary>
        /// Main instance
        /// </summary>
        public static AndroidService Instance { get; private set; }

        /// <summary>
        /// The Discord client
        /// </summary>
        public readonly DiscordSocketClient Client = new DiscordSocketClient();

        /// <summary>
        /// All existing Android instances
        /// </summary>
        public Dictionary<ulong, AndroidInstance> AndroidInstances = new Dictionary<ulong, AndroidInstance>();

        /// <summary>
        /// Construct a service
        /// </summary>
        public AndroidService()
        {
            if (!File.Exists(BotTokenFilePath))
                throw new Exception("Could not find bot token file at " + BotTokenFilePath);

            Instance = this;
        }

        /// <summary>
        /// Start the main Discord loop
        /// </summary>
        public async Task StartDiscordLoop()
        {
            Client.Log += (e) =>
            {
                Console.WriteLine(e.ToString());
                return Task.CompletedTask;
            };

            var botToken = File.ReadAllText(BotTokenFilePath);

            await Client.LoginAsync(TokenType.Bot, botToken);
            await Client.StartAsync();

            Client.GuildAvailable += OnGuildAvailable;
            Client.GuildUnavailable += OnGuildUnavailable;

            await Task.Delay(-1);
        }

        private Task OnGuildUnavailable(SocketGuild arg)
        {
            if (AndroidInstances.TryGetValue(arg.Id, out var instance))
            {
                instance.Shutdown();
                AndroidInstances.Remove(arg.Id);
            }
            return Task.CompletedTask;
        }

        private Task OnGuildAvailable(SocketGuild arg)
        {
            if (AndroidInstances.TryGetValue(arg.Id, out var existing))
            {
                existing.Shutdown();
                AndroidInstances.Remove(arg.Id);
            }

            var instance = new AndroidInstance(arg.Id);

            if (AndroidStateSerialiser.Load(instance.GuildID, out var state))
            {
                foreach (var item in state.Listeners)
                {
                    item.Android = instance;
                    item.Initialise();
                }

                instance.Active = state.Active;
                instance.Listeners.AddRange(state.Listeners);
                instance.Moderation.MutesByUser = state.MutedUsers;
            }

            AndroidInstances.Add(arg.Id, instance);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Find a listener by ID
        /// </summary>
        public AndroidListener FindListener(string id)
        {
            foreach (var instance in AndroidInstances)
                foreach (var listener in instance.Value.Listeners)
                    if (listener.ID == id)
                        return listener;

            return null;
        }

        /// <summary>
        /// Get all the channel infos for a guild
        /// </summary>
        public async Task<IEnumerable<ChannelInfo>> GetChannelsFrom(ulong guildID)
        {
            if (Client.ConnectionState != ConnectionState.Connected)
                return null;

            var guild = await Client.Rest.GetGuildAsync(guildID);
            var channels = await guild.GetChannelsAsync();

            return channels.Select(c =>
            {
                string type = null;
                string description = null;
                string categoryId = null;

                if (c is ITextChannel tc)
                {
                    type = "text";
                    description = tc.Topic;
                    categoryId = tc.CategoryId.ToString();
                }
                else if (c is IVoiceChannel vc)
                {
                    type = "voice";
                    categoryId = vc.CategoryId.ToString();
                }
                else if (c is ICategoryChannel cc)
                {
                    type = "category";
                }

                return new ChannelInfo
                {
                    ID = c.Id.ToString(),
                    Name = c.Name,
                    Description = description,
                    ChannelType = type,
                    Position = c.Position,
                    CategoryID = categoryId
                };
            });
        }

        /// <summary>
        /// Get all available guild infos
        /// </summary>
        public IEnumerable<GuildInfo> GetGuildInfos()
        {
            if (Client.ConnectionState != ConnectionState.Connected)
                return null;

            return Client.Guilds.Select(g => new GuildInfo
            {
                ID = g.Id.ToString(),
                Name = g.Name,
                IconURL = g.IconUrl
            });
        }

        /// <summary>
        /// Save all instances and stop everything
        /// </summary>
        public void Stop()
        {
            AndroidStateSerialiser.SaveAll(this);

            Task.Run(Client.StopAsync).Wait();
        }
    }
}
