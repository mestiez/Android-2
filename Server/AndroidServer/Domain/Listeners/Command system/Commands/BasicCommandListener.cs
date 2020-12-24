using Discord;
using Discord.WebSocket;
using Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AndroidServer.Domain.Listeners.Commands
{
    /// <summary>
    /// Basic bot commands with no significant purpose
    /// </summary>
    public class BasicCommandListener : CommandContainerListener
    {
        public BasicCommandListener(AndroidInstance android, ulong channelID) : base(android, channelID) { }

        private readonly Random rand = new Random();

        [Command(CommandAccessLevel.Level1, "nothing", "no", "nevermind", "nvm", "shut", "ignore", "cancel")]
        public async Task DissmissingResponse(CommandParameters parameters)
        {
            await parameters.Reply("ok");
        }

        [Command(CommandAccessLevel.Level1, "hello", "hi", "hey", "o/", "\\o", "oi", "hey", ":)", "greetings")]
        public async Task GreetingResponse(CommandParameters parameters)
        {
            var responses = new[]{
                    "hi",
                    ":)",
                    "hello",
                    "o/",
                    "o//",
                    "hi :)"
                };

            var i = rand.Next(0, responses.Length);

            await parameters.Reply(responses[i]);
        }

        [Command(CommandAccessLevel.Level3, "archive")]
        public async Task ArchiveChannel(CommandParameters parameters)
        {
            const int MaxMessageCount = 10000;

            var channels = parameters.SocketMessage.MentionedChannels;
            if (channels == null || channels.Count == 0)
            {
                var a = new List<SocketGuildChannel>() { parameters.SocketMessage.Channel as SocketGuildChannel };
                channels = a.AsReadOnly();
            }

            var now = DateTime.UtcNow;

            await parameters.Reply("working on it...");
            var isTyping = parameters.SocketMessage.Channel.EnterTypingState();

            foreach (var item in channels)
            {
                if (item is not ITextChannel tc)
                    continue;

                string file = string.Empty;
                var messages = tc.GetMessagesAsync(MaxMessageCount, CacheMode.AllowDownload);

                await messages.ForEachAwaitAsync(m =>
                {
                    foreach (var item in m)
                        file = file.Insert(0, $"{item.Author} ({item.Timestamp.UtcDateTime})\n\t{item.Content}\n\n");
                    return Task.CompletedTask;
                });

                file = file.Insert(0, $"#{item.Name} at {now}\nBiscuit can only archive the last {MaxMessageCount} messages\n\n");

                await Utilities.SendTextAsFile(parameters.SocketMessage.Channel, file, $"{item.Name}.txt");
            }

            isTyping.Dispose();
        }
    }
}
