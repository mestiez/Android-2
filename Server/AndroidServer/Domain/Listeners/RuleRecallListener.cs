using Discord;
using Discord.WebSocket;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AndroidServer.Domain.Listeners
{
    /// <summary>
    /// Recalls rules when a rule is mentioned
    /// </summary>
    public class RuleRecallListener : AndroidListener
    {
        private ulong ruleMessageID;
        private ulong ruleChannelID;
        private bool validChannels;
        private bool dirty = true;

        [UiVariableType(VariableType.TextChannel)]
        public string RuleMessageChannelID { get; set; } = "";
        [UiVariableType(VariableType.String)]
        public string RuleMessageID { get; set; } = "";
        [UiVariableType(VariableType.String)]
        public string MessageRegex { get; set; } = "rule ?((# ?)|(number )?)\\d+";
        [UiVariableType(VariableType.String)]
        public string RuleRegex { get; set; } = "\\d+ - .+";

        private string[] rules = null;
        private Regex regex;

        public RuleRecallListener(AndroidInstance android, ulong channelID) : base(android, channelID) { }

        public override void Initialise()
        {
            Android.Client.MessageUpdated += OnMessageEdited;
        }

        public override void OnDelete()
        {
            Android.Client.MessageUpdated -= OnMessageEdited;
        }

        public override async Task OnMessage(SocketMessage arg)
        {
            if (dirty)
            {
                dirty = false;
                Configure();
                try
                {
                    rules = await ReadAllRules();
                    validChannels = true;
                }
                catch (Exception)
                {
                    validChannels = false;
                }
            }

            if (!validChannels)
                return;

            var message = arg.Content;
            string response = "";
            foreach (Match match in regex.Matches(message))
            {
                string digitMessage = new string(match.Value.Where(c => char.IsDigit(c)).ToArray()).Trim();
                bool parseSuccess = int.TryParse(digitMessage, out int ruleNr);
                bool validRange = rules.Length >= ruleNr && ruleNr > 0;

                if (parseSuccess && validRange)
                    response += $"rule {rules[ruleNr - 1].ToLower()}\n";

            }

            if (!string.IsNullOrWhiteSpace(response))
                await arg.Channel.SendMessageAsync(response);
        }

        private Task OnMessageEdited(Cacheable<IMessage, ulong> c, SocketMessage message, ISocketMessageChannel channel)
        {
            dirty = true;
            return Task.CompletedTask;
        }

        private void Configure()
        {
            regex = new Regex(MessageRegex, RegexOptions.IgnoreCase);
            if (!ulong.TryParse(RuleMessageID, out ruleMessageID)) ruleMessageID = 0;
            if (!ulong.TryParse(RuleMessageChannelID, out ruleChannelID)) ruleChannelID = 0;
        }

        private async Task<string[]> ReadAllRules()
        {
            var channel = Android.Client.GetChannel(ruleChannelID) as ISocketMessageChannel;
            var message = await channel.GetMessageAsync(ruleMessageID);
            string messageContent = message.Content;
            Regex ruleRegex = new Regex(RuleRegex);
            MatchCollection ruleLines = ruleRegex.Matches(messageContent);
            return ruleLines.Select(r => r.Value).ToArray();
        }

        public override void OnPropertyChange(string name, object value)
        {
            dirty = true;
        }
    }
}
