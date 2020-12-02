﻿using Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AndroidServer.Domain
{
    /// <summary>
    /// Manages mutes and channel bans for an Android instance
    /// </summary>
    public class AndroidModerationManager
    {
        /// <summary>
        /// Associated guild
        /// </summary>
        public IGuild Guild { get; }

        /// <summary>
        /// The ID of the mute role. Usually set by <see cref="Listeners.Commands.ModerationCommandListener"/>
        /// </summary>
        public ulong MutedRoleID
        {
            get => mutedRoleID;

            set
            {
                mutedRoleID = value;
                mutedRole = null;
            }
        }

        /// <summary>
        /// Raw mute entries by user ID. Should not be directly edited
        /// </summary>
        public Dictionary<ulong, MuteEntry> MutesByUser = new Dictionary<ulong, MuteEntry>();

        private Timer timer;
        private ulong mutedRoleID;
        private readonly AndroidInstance instance;
        private IRole mutedRole = null;

        /// <summary>
        /// Construct a moderation manager
        /// </summary>
        public AndroidModerationManager(AndroidInstance instance)
        {
            this.instance = instance;
            Guild = instance.Guild;
        }

        /// <summary>
        /// Initialise the manager
        /// </summary>
        public void Initialise()
        {
            timer = new Timer(CheckExpiredEntries, null, TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(1));
        }

        private void RetrieveMutedRole()
        {
            mutedRole = Guild.GetRole(mutedRoleID);
        }

        private async void CheckExpiredEntries(object state)
        {
            if (MutesByUser.Count == 0) return;
            var now = DateTime.UtcNow;
            foreach (var pair in MutesByUser.ToArray())
            {
                var expired = now >= pair.Value.Expiration;
                if (expired)
                {
                    var user = await Guild.GetUserAsync(pair.Key);
                    await Unmute(user);
                }
            }
        }

        /// <summary>
        /// Unmute a user
        /// </summary>
        public async Task Unmute(IGuildUser user)
        {
            ulong id = user.Id;

            if (!MutesByUser.TryGetValue(id, out var entry)) return;

            var removalSuccess = MutesByUser.Remove(id);
            if (!removalSuccess)
                Console.WriteLine("Could not remove " + id + " from the mute entry list");

            if (user == null)
            {
                Console.WriteLine("User with ID " + id + " is null");
                return;
            }

            await SetMuteState(user, false);
            var channel = instance.Client.GetChannel(entry.ChannelID) as ITextChannel;
            await channel.SendMessageAsync("unmuting " + user.Username);
        }

        /// <summary>
        /// Mute a user
        /// </summary>
        public async Task Mute(IGuildUser user, ITextChannel channel, TimeSpan duration)
        {
            bool userIsAlreadyMuted = MutesByUser.TryGetValue(user.Id, out var entry);
            if (userIsAlreadyMuted)
            {
                var newExpiration = DateTime.UtcNow + duration;
                var isLonger = newExpiration > entry.Expiration;
                entry.Expiration = newExpiration;
                entry.ChannelID = channel.Id;
                await channel.SendMessageAsync(isLonger ? "extending mute..." : "shortening mute...");
            }
            else
            {
                if (user == null)
                {
                    Console.WriteLine("User with ID " + user.Id + " is null");
                    return;
                }
                MutesByUser.Add(user.Id, new MuteEntry(user.Id, channel.Id, user.Guild.Id, DateTime.UtcNow + duration));
                await channel.SendMessageAsync("muting " + user.Username);
            }
            await SetMuteState(user, true);
        }

        private async Task SetMuteState(IGuildUser user, bool muted)
        {
            if (mutedRole == null)
                RetrieveMutedRole();

            if (mutedRole == null)
                return;

            if (muted)
                await user.AddRoleAsync(mutedRole);
            else
                await user.RemoveRoleAsync(mutedRole);
        }

        /// <summary>
        /// Set a channel specific ban. This will fail if there is no role that exists specifically for this channel ban
        /// </summary>
        public async Task<bool> SetChannelBan(IGuildChannel channel, IGuildUser user, bool ban, IEnumerable<IGuildChannel> allChannels = null)
        {
            if (allChannels == null)
                allChannels = await Guild.GetChannelsAsync();

            Dictionary<ulong, List<IGuildChannel>> channelBanPerRole = new();

            foreach (var c in allChannels)
            {
                foreach (var perm in c.PermissionOverwrites)
                {
                    if (perm.TargetType != PermissionTarget.Role)
                        continue;

                    if (c is ITextChannel && perm.Permissions.SendMessages != PermValue.Deny)
                        continue;

                    if (c is IVoiceChannel && perm.Permissions.Speak != PermValue.Deny)
                        continue;

                    if (channelBanPerRole.TryGetValue(perm.TargetId, out var list))
                        list.Add(c);
                    else
                        channelBanPerRole.Add(perm.TargetId, new List<IGuildChannel> { c });
                }
            }

            bool pardonSuccess = false;
            foreach (var pair in channelBanPerRole)
            {
                var channels = pair.Value;
                var roleId = pair.Key;

                if (channels.Count != 1 || !channels.Any(c => c.Id == channel.Id))
                    continue;

                var role = Guild.GetRole(roleId);

                if (ban)
                {
                    await user.AddRoleAsync(role);
                    return true;
                }
                else
                {
                    await user.RemoveRoleAsync(role);
                    pardonSuccess = true;
                }
            }

            if (!ban && pardonSuccess)
                return true;

            return false;
        }
    }
}