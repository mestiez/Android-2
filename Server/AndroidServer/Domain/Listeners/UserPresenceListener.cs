using Discord.WebSocket;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace AndroidServer.Domain.Listeners
{
    /// <summary>
    /// Sends join/leave/ban/unban messages to a channel
    /// </summary>
    public class UserPresenceListener : AndroidListener
    {
        public UserPresenceListener(AndroidInstance android, ulong channelID) : base(android, channelID) { }

        [UiVariableType(VariableType.TextChannel)]
        public string LogChannelID { get; set; }

        public override void Initialise()
        {
            Android.Client.UserJoined += OnUserJoined;
            Android.Client.UserLeft += OnUserLeft;
            Android.Client.UserBanned += OnUserBanned;
            Android.Client.UserUnbanned += OnUserUnbanned;
        }

        public override void OnDelete()
        {
            OnShutdown();
        }

        public override void OnShutdown()
        {
            Android.Client.UserJoined -= OnUserJoined;
            Android.Client.UserLeft -= OnUserLeft;
            Android.Client.UserBanned -= OnUserBanned;
            Android.Client.UserUnbanned -= OnUserUnbanned;
        }

        private async Task OnUserUnbanned(SocketUser user, SocketGuild guild)
        {
            if (!Active || !Android.Active) return;

            if (guild.Id != Android.GuildID)
                return;

            await Append($"{user.Username}#{user.Discriminator} unbanned on {DateTime.UtcNow.ToString()}");
            await Task.CompletedTask;
        }

        private async Task OnUserBanned(SocketUser user, SocketGuild guild)
        {
            if (!Active || !Android.Active) return;

            if (guild.Id != Android.GuildID)
                return;

            string banReason = "";
            try
            {
                var ban = await guild.GetBanAsync(user);
                banReason = " for " + (string.IsNullOrWhiteSpace(ban.Reason) ? "no reason" : ("\"" + ban.Reason + "\""));
            }
            catch (Exception)
            {
                Console.WriteLine("Failed to get ban reason for " + user.Username);
            }
            await Append($"{user.Username}#{user.Discriminator} banned on {DateTime.UtcNow}{banReason}");
            await Task.CompletedTask;
        }

        private async Task OnUserLeft(SocketGuildUser user)
        {
            if (!Active || !Android.Active) return;

            if (user.Guild.Id != Android.GuildID)
                return;


            string leaveMessage = $"{user.Username}#{user.Discriminator} left on {DateTime.UtcNow}";
            var roles = user.Roles.Where(r => !r.IsEveryone).ToList();
            if (roles.Count > 0)
                leaveMessage += $"\n**Left with roles:** {string.Join(", ", roles.Select(s => s.Name))}";

            await Append(leaveMessage);
            await Task.CompletedTask;
        }

        private async Task OnUserJoined(SocketGuildUser user)
        {
            if (!Active || !Android.Active) return;

            if (user.Guild.Id != Android.GuildID)
                return;

            var age = (DateTime.UtcNow - user.CreatedAt.UtcDateTime);
            string timespanString;

            if (age.TotalDays > 1)
                timespanString = $"{Math.Round(age.TotalDays, 1)} days";
            else if (age.TotalHours > 1)
                timespanString = $"{Math.Round(age.TotalHours, 1)} hours";
            else if (age.TotalMinutes > 1)
                timespanString = $"{Math.Round(age.TotalMinutes, 1)} minutes";
            else
                timespanString = $"{Math.Round(age.TotalSeconds, 1)} seconds";

            var a = $"<@{user.Id}> joined on {DateTime.UtcNow}\nThis account is {timespanString} old";

            await Append(a);

            //TODO mute system
            //if (MuteSystem.IsMuted(user.Id))
            //    await user.AddRoleAsync(android.MainGuild.GetRole(Server.Roles.Muted));

            await Task.CompletedTask;
        }

        private async Task Append(string content)
        {
            if (ulong.TryParse(LogChannelID, out var channelId))
                await (Android.Client.GetChannel(channelId) as ISocketMessageChannel).SendMessageAsync(content);

            await Task.CompletedTask;
        }
    }
}
