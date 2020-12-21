using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace AndroidServer.Domain.Listeners
{
    /// <summary>
    /// Sends messages when a message is pinned
    /// </summary>
    public class PinListener : AndroidListener
    {
        public PinListener(AndroidInstance android, ulong channelID) : base(android, channelID) { }

        [UiVariableType(VariableType.TextChannel)]
        public string LogChannelID { get; set; }

        [Newtonsoft.Json.JsonRequired]
        private HashSet<ulong> ignoreFurtherEdit = new HashSet<ulong>();

        private int c = 0;

        public override void Initialise()
        {
            Android.Client.MessageUpdated += OnMessageEdited;
        }

        public override void OnShutdown()
        {
            Android.Client.MessageUpdated -= OnMessageEdited;
        }

        private async Task OnMessageEdited(Cacheable<IMessage, ulong> c, SocketMessage message, ISocketMessageChannel channel)
        {
            if ((channel.Id != ChannelID && !GlobalListener) || (channel as IGuildChannel).GuildId != Android.GuildID || !Active || !Android.Active) 
                return;

            if (ulong.TryParse(LogChannelID, out var channelId) && message.IsPinned && !ignoreFurtherEdit.Contains(message.Id))
            {
                var embed = new EmbedBuilder().
                    WithTitle("Pinned message").
                    WithDescription($"[Jump to message]({message.GetJumpUrl()})").
                    AddField("Channel", message.Channel.Name).
                    AddField("Author", message.Author).
                    AddField("Original content", message.Content).
                    Build();

                ignoreFurtherEdit.Add(message.Id);
                await (Android.Client.GetChannel(channelId) as ISocketMessageChannel).SendMessageAsync(embed: embed);
            }
        }
    }
}
