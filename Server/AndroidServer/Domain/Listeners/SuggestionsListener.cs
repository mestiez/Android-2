using Discord;
using Discord.WebSocket;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;

namespace AndroidServer.Domain.Listeners
{
    /// <summary>
    /// A listener that registers suggestions and ranks them using emote-voting
    /// </summary>
    public class SuggestionsListener : AndroidListener
    {
        public SuggestionsListener(AndroidInstance android, ulong channelID) : base(android, channelID) { }

        public Dictionary<ulong, Suggestion> Suggestions { get; set; }

        [UiVariableType(VariableType.String)]
        public string SuggestionPrefix { get; set; } = "suggestion:";
        [UiVariableType(VariableType.Boolean)]
        public bool CaseSensitivePrefix { get; set; } = false;
        [UiVariableType(VariableType.EmoteID)]
        public string PositiveVoteEmoteID { get; set; }
        [UiVariableType(VariableType.EmoteID)]
        public string NegativeVoteEmoteID { get; set; }
        [UiVariableType(VariableType.Boolean)]
        public bool DoWeeklyReset { get; set; } = true;
        [UiVariableType(VariableType.Number)]
        public float WeeklyResetWeekday
        {
            get => weeklyResetWeekday;
            set
            {
                int d = (int)MathF.Round(MathF.Max(0, MathF.Min(value, 7)));
                weeklyResetWeekday = d;
            }
        }

        private readonly Timer timer = new Timer();
        private bool canReset = true;
        private int weeklyResetWeekday = 2;

        private IEmote upvote;
        private IEmote downvote;

        public override void Initialise()
        {
            if (Suggestions == null)
                Suggestions = new Dictionary<ulong, Suggestion>();

            Android.Client.ReactionAdded += OnReactionAdd;
            Android.Client.ReactionRemoved += OnReactionRemoved;

            timer.Interval = TimeSpan.FromMinutes(60).TotalMilliseconds;

            timer.Elapsed += CheckForReset;

            timer.Start();
        }

        private async void CheckForReset(object o, ElapsedEventArgs ee)
        {
            if (!Android.Active || !Active)
                return;

            var now = DateTime.UtcNow;
            if (now.DayOfWeek == (DayOfWeek)weeklyResetWeekday && now.TimeOfDay.Hours == 12)
            {
                try
                {
                    if (!canReset)
                    {
                        Console.WriteLine("Attempt to reset suggestions but canReset was false");
                        return;
                    }
                    canReset = false;
                    await ResetPeriodicBoard();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
            }
            else canReset = true;
        }

        protected override void OnEnable()
        {
            timer.Start();
        }

        protected override void OnDisable()
        {
            timer.Stop();
        }

        public override void OnDelete()
        {
            OnShutdown();
        }

        private async Task RetrieveEmotes()
        {

            if (upvote == null && ulong.TryParse(PositiveVoteEmoteID, out ulong u))
                upvote = await Android.Guild.GetEmoteAsync(u);

            if (downvote == null && ulong.TryParse(NegativeVoteEmoteID, out u))
                downvote = await Android.Guild.GetEmoteAsync(u);
        }

        public override void OnShutdown()
        {
            timer.Stop();
            Android.Client.ReactionAdded -= OnReactionAdd;
            Android.Client.ReactionRemoved -= OnReactionRemoved;
            timer.Elapsed -= CheckForReset;
            timer.Dispose();
        }

        private async Task OnReactionRemoved(Cacheable<IUserMessage, ulong> message, ISocketMessageChannel channel, SocketReaction reaction)
        {
            if (channel.Id != ChannelID) return;
            if (!Suggestions.TryGetValue(message.Id, out var suggestion)) return;

            await RetrieveEmotes();

            if (reaction.Emote.Equals(upvote))
                suggestion.Upvotes--;
            else if (reaction.Emote.Equals(downvote))
                suggestion.Downvotes--;

            await Task.CompletedTask;
        }

        private async Task OnReactionAdd(Cacheable<IUserMessage, ulong> message, ISocketMessageChannel channel, SocketReaction reaction)
        {
            if (channel.Id != ChannelID) return;
            if (!Suggestions.TryGetValue(message.Id, out var suggestion)) return;

            await RetrieveEmotes();

            if (reaction.Emote.Equals(upvote))
                suggestion.Upvotes++;
            else if (reaction.Emote.Equals(downvote))
                suggestion.Downvotes++;

            await Task.CompletedTask;
        }

        public override async Task OnMessageEdited(IMessage arg)
        {
            if (Suggestions.ContainsKey(arg.Id))
                await arg.Channel.SendMessageAsync("do not edit your suggestion, especially if it was marked a duplicate");
        }

        public override async Task OnMessage(SocketMessage arg)
        {
            await CheckNewSuggestion(arg);
        }

        private async Task CheckNewSuggestion(SocketMessage arg)
        {
            if (!IsSuggestion(arg.Content)) return;

            if (IsDuplicate(arg.Content))
            {
                await arg.Channel.SendMessageAsync("that has been suggested before and **any attempt to bypass a duplicate check will result in a channel ban**");
                return;
            }

            if (arg.Content.Length > 1024)
                await arg.Channel.SendMessageAsync("A suggestion shouldn't exceed 1024 characters. Try not to group multiple suggestions into a single message.");

            await RetrieveEmotes();
            
            await arg.AddReactionAsync(upvote);
            await arg.AddReactionAsync(downvote);
            AddSuggestion((IUserMessage)arg, false);
        }

        public void AddSuggestion(IUserMessage message, bool doReactionCheck = true)
        {
            if (!IsSuggestion(message.Content)) return;

            if (IsDuplicate(message.Content))
            {
                Console.WriteLine("Duplicate omitted");
                return;
            }

            var reactions = message.Reactions;
            var hasUpvotes = reactions.TryGetValue(upvote, out var upvoteMetadata);
            var hasDownvotes = reactions.TryGetValue(downvote, out var downvoteMetadata);
            if ((!hasUpvotes || !hasDownvotes) && doReactionCheck) return;

            Suggestions.Add(message.Id, new Suggestion(upvoteMetadata.ReactionCount, downvoteMetadata.ReactionCount, message.Author.Id, message.Content));
        }

        private bool IsSuggestion(string content)
        {
            if (CaseSensitivePrefix)
                return (content.Trim().StartsWith(SuggestionPrefix));
            else
                return (content.Trim().ToLower().StartsWith(SuggestionPrefix.ToLower()));
        }

        private bool IsDuplicate(string suggestion)
        {
            string keyContent = GetSignificantContent(suggestion);

            return Suggestions.Any(s =>
            {
                string value = GetSignificantContent(s.Value.Content);

                return (value == keyContent) /*|| levenshtein.Distance(value, keyContent) < .1f*/;
            });
        }

        private string GetSignificantContent(string input)
        {
            //TODO remove insignificance
            return input.Normalize().ToLower().Trim();
        }

        public async Task ResetPeriodicBoard()
        {
            var channel = await Android.Guild.GetTextChannelAsync(ChannelID);

            await channel.SendMessageAsync("weekly suggestion reset");

            var (embed, count) = CreateLeaderboardEmbed();
            if (count != 0)
                await channel.SendMessageAsync($"the top {count} suggestions the past week were", false, embed);

            await channel.SendMessageAsync("resetting...");

            Suggestions.Clear();

            await channel.SendMessageAsync("suggestions reset (≧◡≦)");
        }

        private (Embed embed, int count) CreateLeaderboardEmbed()
        {
            var values = Suggestions.Values;
            var topSuggestions = values.OrderByDescending(s => s.Score).Take(25);
            var builder = new EmbedBuilder();
            builder.Color = new Color(0x7289da);
            int index = 0;
            foreach (var suggestion in topSuggestions)
            {
                index++;
                builder.AddField($"#{index}: {suggestion.Score} points", suggestion.EllipsedContent);
            }
            return (builder.Build(), topSuggestions.Count());
        }

        [Serializable]
        public class Suggestion
        {
            public int Upvotes;
            public int Downvotes;
            public ulong AuthorId;
            public string Content;

            public Suggestion(int upvotes, int downvotes, ulong authorId, string content)
            {
                Upvotes = upvotes;
                Downvotes = downvotes;
                AuthorId = authorId;
                Content = content;
            }

            [JsonIgnore]
            public int Score => Upvotes - Downvotes;

            [JsonIgnore]
            public string EllipsedContent
            {
                get
                {
                    string ellipsed = Content;
                    if (Content.Length > 1024)
                        ellipsed = Content.Substring(0, 1021) + "...";
                    return ellipsed;
                }
            }
        }
    }
}
