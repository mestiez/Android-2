using Discord;
using Discord.WebSocket;
using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AndroidServer.Domain.Listeners
{
    /// <summary>
    /// Mutes a user when they spam ping
    /// </summary>
    public class MentionSpamListener : AndroidListener
    {
        [UiVariableType(VariableType.Number)]
        public float MentionCountThreshold { get; set; } = 5;
        [UiVariableType(VariableType.Number)]
        public float MuteDurationInHours { get; set; } = 24;

        private readonly Regex tester = new Regex(@"<@\d+>");

        public MentionSpamListener(AndroidInstance android, ulong channelID) : base(android, channelID) { }

        public override async Task OnMessage(SocketMessage message)
        {
            var c = CountMentions(message.Content);
            Console.WriteLine($"message has {c} mentions");
            if (c > MentionCountThreshold)
                await Punish(message);
        }

        private int CountMentions(string content)
        {
            if (content.Length <= 4) //min ID length
                return 0;

            var matches = tester.Matches(content);
            return matches.Count;
        }

        private async Task Punish(SocketMessage message)
        {
            await message.Channel.SendMessageAsync($"mentioning over {(int)MathF.Floor(MentionCountThreshold)} users in one message is against the rules\nyou will be muted for {MuteDurationInHours} hours");
            await Android.Moderation.Mute(message.Author as IGuildUser, message.Channel as ITextChannel, TimeSpan.FromHours(MuteDurationInHours));
        }
    }
}
