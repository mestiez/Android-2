using Discord;
using Discord.WebSocket;
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

        public override async Task OnMessageEdited(IMessage message)
        {
            if (message.IsPinned && !ignoreFurtherEdit.Contains(message.Id))
            {
                var embed = new EmbedBuilder().
                    WithTitle("Pinned message").
                    WithDescription($"[Jump to message]({message.GetJumpUrl()})").
                    AddField("Channel", message.Channel.Name).
                    AddField("Author", message.Author).
                    AddField("Original content", message.Content).
                    Build();

                if (ulong.TryParse(LogChannelID, out var channelId))
                {
                    await (Android.Client.GetChannel(channelId) as ISocketMessageChannel).SendMessageAsync(embed: embed);
                    ignoreFurtherEdit.Add(message.Id);
                }
            }
            await Task.CompletedTask;
        }
    }
}
