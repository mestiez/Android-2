using Discord;
using Domain;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace AndroidServer.Domain.Listeners.Commands
{
    /// <summary>
    /// All moderation commands. Also where the mute role is configured by the client
    /// </summary>
    public class ModerationCommandListener : CommandContainerListener
    {
        private ulong mutedRole;
        private ulong interrogationRole;

        [UiVariableType(VariableType.RoleID)]
        public string MutedRole
        {
            get
            {
                return mutedRole.ToString();
            }

            set
            {
                if (ulong.TryParse(value, out var result))
                {
                    mutedRole = result;
                    if (Android != null && Android.Moderation != null)
                        Android.Moderation.MutedRoleID = result;
                }
            }
        }

        [UiVariableType(VariableType.RoleID)]
        public string InterrogationRole
        {
            get
            {
                return interrogationRole.ToString();
            }

            set
            {
                if (ulong.TryParse(value, out var result))
                    interrogationRole = result;
            }
        }

        public override void Initialise()
        {
            Android.Moderation.MutedRoleID = mutedRole;
        }

        [UiVariableType(VariableType.Number)]
        public float DefaultMuteDurationInMinutes { get; set; } = 15;

        public ModerationCommandListener(AndroidInstance android, ulong channelID) : base(android, channelID) { }

        [Command(CommandAccessLevel.Level1, "ban")]
        public async Task BanFromChannels(CommandParameters parameters)
        {
            await SetChannelBan(parameters, true);
        }

        [Command(CommandAccessLevel.Level1, "unban")]
        public async Task UnbanFromChannels(CommandParameters parameters)
        {
            await SetChannelBan(parameters, false);
        }

        [Command(CommandAccessLevel.Level1, "interrogate", "question")]
        public async Task Interrogate(CommandParameters parameters)
        {
            await SetInterrogationStatus(parameters, true);
        }

        [Command(CommandAccessLevel.Level1, "release", "uninterrogate")]
        public async Task Uninterrogate(CommandParameters parameters)
        {
            await SetInterrogationStatus(parameters, false);
        }

        private async Task SetInterrogationStatus(CommandParameters parameters, bool state)
        {
            var users = parameters.SocketMessage.MentionedUsers;
            var role = Android.Guild.GetRole(interrogationRole);

            if (role == null)
            {
                await parameters.Reply("no interrogation role set... should probably tell an admin");
                return;
            }

            foreach (var item in users)
            {
                var gu = await Android.Guild.GetUserAsync(item.Id);
                if (state)
                    await gu.AddRoleAsync(role);
                else
                    await gu.RemoveRoleAsync(role);
            }

            if (state)
            {
                string reply;
                const string suffix = " ready for interrogation";
                if (users.Count > 1)
                    reply = string.Join(", ", users.Select(u => u.Username)) + " are" + suffix;
                else
                    reply = users.First().Username + " is" + suffix;

                await parameters.Reply(reply);
            }
            else 
                await parameters.Reply("come back soon!");
        }

        [Command(CommandAccessLevel.Level1, "mute")]
        public async Task Mute(CommandParameters parameters)
        {
            var users = parameters.SocketMessage.MentionedUsers;
            var channel = parameters.SocketMessage.Channel;

            if (users == null || users.Count == 0)
            {
                await parameters.Reply("no users specified");
                return;
            }

            var parsedDuration = Utilities.ParseTimeFromText(parameters.ContentWithoutTriggerAndCommand);
            var duration = parsedDuration ?? TimeSpan.FromMinutes(DefaultMuteDurationInMinutes);

            if (!parsedDuration.HasValue)
                await parameters.Reply("no duration specified, falling back to " + DefaultMuteDurationInMinutes + " minutes");

            foreach (var user in users)
            {
                await Android.Moderation.Mute(user as IGuildUser, channel as ITextChannel, duration);
            }
        }

        [Command(CommandAccessLevel.Level1, "unmute")]
        public async Task Unmute(CommandParameters parameters)
        {
            var users = parameters.SocketMessage.MentionedUsers;

            if (users == null || users.Count == 0)
            {
                await parameters.Reply("no users specified");
                return;
            }

            foreach (var user in users)
            {
                await Android.Moderation.Unmute(user as IGuildUser);
            }
        }

        [Command(CommandAccessLevel.Level1, "list all mutes", "list mutes", "mutes", "who is muted", "who's muted")]
        public async Task ListAllMutes(CommandParameters parameters)
        {
            var mutes = Android.Moderation.MutesByUser;
            if (mutes.Count == 0)
            {
                await parameters.Reply("nobody is muted (excluding manual mutes)");
                return;
            }
            var now = DateTime.UtcNow;
            string toSend = "at the time of this message...\n";
            foreach (var item in mutes)
            {
                var user = Android.Client.GetUser(item.Key);
                if (user == null) continue;

                toSend += $"{user.Username}#{user.Discriminator} is muted for {Utilities.TimeSpanToText(item.Value.Expiration - now)}\n";
            }
            await parameters.Reply(toSend);
        }

        private async Task SetChannelBan(CommandParameters parameters, bool ban)
        {
            var channels = await Android.Guild.GetChannelsAsync();
            var users = parameters.SocketMessage.MentionedUsers;
            var mentionedChannels = parameters.SocketMessage.MentionedChannels;

            foreach (var channel in mentionedChannels)
                foreach (var user in users)
                {
                    var result = await Android.Moderation.SetChannelBan(channel, user as IGuildUser, ban, channels);
                    if (!result)
                        await parameters.Reply($"could not find an appropriate muting role for <#{channel.Id}>");
                }
        }
    }
}
