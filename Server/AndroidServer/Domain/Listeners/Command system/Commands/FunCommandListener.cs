using Discord.WebSocket;
using Domain;
using MathsParser;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using Parser = MathsParser.MathsParser;

namespace AndroidServer.Domain.Listeners.Commands
{
    /// <summary>
    /// """Funny""" commands
    /// </summary>
    public class FunCommandListener : CommandContainerListener
    {
        public FunCommandListener(AndroidInstance android, ulong channelID) : base(android, channelID) { }

        [Command(CommandAccessLevel.Level3, "i love you", "love you", "ilu", "<3")]
        public async Task Love(CommandParameters parameters)
        {
            await parameters.SocketMessage.Channel.SendMessageAsync("<3");
        }

        [Command(CommandAccessLevel.Level2, "lego", "atst", "at-st")]
        public async Task Lego(CommandParameters parameters)
        {
            await parameters.Reply("https://i.imgur.com/YsUVChu.png");
        }

        [Command(CommandAccessLevel.Level2, "evaluate", "calculate", "eval", "calc")]
        public async Task Maths(CommandParameters parameters)
        {
            try
            {
                var result = Parser.Parse(parameters.ContentWithoutTriggerAndCommand.ToLower());

                var safeReply = Utilities.ReplaceMentions(parameters.ContentWithoutTriggerAndCommand);
                await parameters.Reply($"{safeReply} = {result.ToString(CultureInfo.InvariantCulture)}");
            }
            catch (InfinityException)
            {
                await parameters.Reply("infinity");
            }
            catch (NaNException)
            {
                await parameters.Reply("the result was not a number");
            }
            catch (Exception)
            {
                await parameters.Reply("i don't understand the expression");
            }
        }

        [Command(CommandAccessLevel.Level2, "what is", "whats", "what's", "define", "what is the definition of")]
        public async Task Define(CommandParameters parameters)
        {
            if (parameters.ContentWithoutTriggerAndCommand.Any(c => char.IsDigit(c)))
            {
                await Maths(parameters);
                return;
            }

            //https://github.com/meetDeveloper/googleDictionaryAPI
            const string api = @"https://api.dictionaryapi.dev/api/v1/entries/en/";
            var isTyping = parameters.SocketMessage.Channel.EnterTypingState();
            var word = parameters.ContentWithoutTriggerAndCommand;
            try
            {
                var resultArray = await Utilities.HttpGet<DictionaryApiResponse[]>(api + word);
                if (resultArray == null || resultArray.Length == 0)
                {
                    await parameters.SocketMessage.Channel.SendMessageAsync("i don't know");
                    isTyping?.Dispose();
                    return;
                }
                var result = resultArray[0];
                if (result.meaning == null || result.meaning.Count == 0)
                {
                    await parameters.SocketMessage.Channel.SendMessageAsync("i don't know");
                    isTyping?.Dispose();
                    return;
                }
                var reply = $@"**{result.word}**: {result.meaning.First().Value.First().definition}";
                var safeReply = Utilities.ReplaceMentions(reply);
                isTyping?.Dispose();
                await parameters.SocketMessage.Channel.SendMessageAsync(safeReply);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message + "\nwhile trying to get definition of \'" + word + "\'");
                isTyping?.Dispose();
                await parameters.SocketMessage.Channel.SendMessageAsync("i don't know");
                return;
            }
            isTyping?.Dispose();
        }

        [Command(CommandAccessLevel.Level3, "say")]
        public async Task Say(CommandParameters parameters)
        {
            var message = parameters.ContentWithoutTriggerAndCommand;

            bool specifiesChannel = message.EndsWith(">");
            ulong channelId = parameters.SocketMessage.Channel.Id;

            if (specifiesChannel)
            {
                Regex regex = new Regex(@"(in <#\d+>)");
                var matches = regex.Matches(message.ToLower());
                if (matches.Count != 0)
                {
                    var match = matches.Last();
                    bool successfulParse = ulong.TryParse(new string(match.Value.Where(c => char.IsDigit(c)).ToArray()), out channelId);
                    if (!successfulParse)
                        await parameters.Reply("something went wrong");
                    else
                        message = message.Substring(0, message.Length - match.Length);
                }
            }

            var channel = parameters.Instance.Client.GetChannel(channelId);
            if (channel is SocketTextChannel tc)
                await tc.SendMessageAsync(message);
        }

        //JSON SCHEMA. DO NOT EDIT
        struct DictionaryApiResponse
        {
            public string word;
            public Dictionary<string, Meaning[]> meaning;
        }

        //JSON SCHEMA. DO NOT EDIT
        public struct Meaning
        {
            public string definition;
        }
    }
}
