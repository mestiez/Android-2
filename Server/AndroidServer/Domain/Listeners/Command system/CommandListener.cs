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
    /// Listens for commands registed by <see cref="CommandContainerListener"/>s
    /// </summary>
    [HideListenerBase]
    public class CommandListener : AndroidListener
    {
        public CommandListener(AndroidInstance android, ulong channelID) : base(android, channelID) { }

        [UiVariableType(VariableType.RoleID)]
        public string Level1AccessRole { get; set; }
        public ulong Lvl1RoleID;

        [UiVariableType(VariableType.RoleID)]
        public string Level2AccessRole { get; set; }
        public ulong Lvl2RoleID;

        [UiVariableType(VariableType.RoleID)]
        public string Level3AccessRole { get; set; }
        public ulong Lvl3RoleID;

        [UiVariableType(VariableType.RoleID)]
        public string Level4AccessRole { get; set; }
        public ulong Lvl4RoleID;

        [UiVariableType(VariableType.TextArea)]
        public string Prefixes { get; set; } = "\nhey \nay \noy \nyo \nokay \noi \noh \nok \nmr \nmr. \nmr.\nmister \n...\nlmao \nlol \nour \nthe \nmy ";
        private IEnumerable<string> PrefixArray => Prefixes.Split('\n', '\r').OrderByDescending(s => s.Length);

        [UiVariableType(VariableType.TextArea)]
        public string Names { get; set; } = "android\nbot\ndroid\nbiscuit\nbiscuit#1\nbiscuit#2\nbiscuit1\nbiscuit2\nrobotboy\nr2\nr2d2\ncomputer\nslave\nc3po\n3po\nxj9\nnano\nrobot";
        private IEnumerable<string> NameArray => Names.Split('\n', '\r').OrderByDescending(s => s.Length);

        [UiVariableType(VariableType.TextArea)]
        public string Suffixes { get; set; } = "\n~\n ~\n~~\n ~~\n,\n.\n..\n...\n?\n??\n???\n!\n!!\n!!!\nsan\nchan\nkun\nsenpai\n san\n chan\n kun\n senpai\n-san\n-chan\n-kun\n-senpai";
        private IEnumerable<string> SuffixArray => Suffixes.Split('\n', '\r').OrderByDescending(s => s.Length);

        [UiVariableType(VariableType.TextArea)]
        public string TriggerResponses { get; set; } = "hm?\nwhat\n?\nyes?\npresent\nwhat do you WANT\n??\nwhat's up\nyeah?";
        private string[] TriggerResponseArray => TriggerResponses.Split('\n', '\r').ToArray();

        private List<string> generatedTriggers;
        private List<RegisteredCommand> rawCommands = new();
        private List<KeyValuePair<string, RegisteredCommand>> orderedCommands = new();

        private bool dirtyCommands = true;

        public override void Initialise()
        {
            GlobalListener = true;
            LoadTriggers();
            LoadAccessLevelRoles();
        }

        public override async Task OnMessage(SocketMessage message)
        {
            var guildUser = await Android.Guild.GetUserAsync(message.Author.Id);
            int accessLevel = GetAccessLevel(guildUser);

            if (accessLevel == 0)
                return;

            string content = message.Content.Trim().Normalize();

            foreach (var trigger in generatedTriggers)
            {
                if (!content.ToLower().StartsWith(trigger.ToLower())) continue;

                var withoutTrigger = content.Remove(0, trigger.Length).Trim();

                if (withoutTrigger.Length == 0)
                {
                    var response = Utilities.PickRandom(TriggerResponseArray);
                    await Android.Ask(message.Author.Id, message.Channel, RespondToLateTrigger, response);
                    return;
                }

                if (dirtyCommands)
                    RetrieveCommands();

                await ExecuteCommand(withoutTrigger, message, guildUser, accessLevel);

                return;
            }
        }

        private async Task RespondToLateTrigger(SocketMessage message)
        {
            if (dirtyCommands)
                RetrieveCommands();

            var guildUser = await Android.Guild.GetUserAsync(message.Author.Id);
            int accessLevel = GetAccessLevel(guildUser);

            await ExecuteCommand(message.Content, message, guildUser, accessLevel);
        }

        private void RetrieveCommands()
        {
            rawCommands.Clear();

            var cmdContainers = Android.GetListeners<CommandContainerListener>();

            foreach (var container in cmdContainers)
            {
                if (!container.Active)
                    continue;

                var commands = container.RetrieveCommands();
                rawCommands.AddRange(commands);
            }

            dirtyCommands = false;
            ReorderCommands();
        }

        public override void OnPropertyChange(string name, object value)
        {
            switch (name)
            {
                case nameof(Prefixes):
                case nameof(Names):
                case nameof(Suffixes):
                    LoadTriggers();
                    break;
                case nameof(Level1AccessRole):
                case nameof(Level2AccessRole):
                case nameof(Level3AccessRole):
                case nameof(Level4AccessRole):
                    LoadAccessLevelRoles();
                    break;
            }
        }

        private async Task ExecuteCommand(string withoutTrigger, SocketMessage message, IGuildUser guildUser, int accessLevel)
        {
            foreach (var pair in orderedCommands)
            {
                var cmd = pair.Value;
                var alias = pair.Key;

                if (!withoutTrigger.ToLower().StartsWith(alias)) continue;
                if (cmd.Attribute.AccessLevel > accessLevel)
                {
                    await message.Channel.SendMessageAsync("unauthorised");
                    continue;
                }

                if (!cmd.Listener.GlobalListener && message.Channel.Id != cmd.Listener.ChannelID)
                    continue;

                if (!cmd.Listener.ResponseFilters.PassesFilter(guildUser, message))
                    continue;

                var withoutAliasAndTrigger = withoutTrigger.Remove(0, alias.Length).Trim();
                var arguments = withoutAliasAndTrigger.Split(' ');

                var parameters = new CommandParameters
                {
                    ContentWithoutTrigger = withoutTrigger,
                    ContentWithoutTriggerAndCommand = withoutAliasAndTrigger,
                    GuildUser = guildUser,
                    AccessLevel = accessLevel,
                    SocketMessage = message,
                    GivenArguments = arguments,
                    Instance = Android
                };

                await cmd.Delegate(parameters);

                return;
            }
        }

        private void LoadTriggers()
        {
            generatedTriggers = new List<string>();

            foreach (var middle in NameArray)
                foreach (var begin in PrefixArray)
                    foreach (var end in SuffixArray)
                        generatedTriggers.Add(begin + middle + end);

            generatedTriggers = generatedTriggers.OrderByDescending(s => s.Length).ToList();
        }

        private void LoadAccessLevelRoles()
        {
            register(Level1AccessRole, ref Lvl1RoleID);
            register(Level2AccessRole, ref Lvl2RoleID);
            register(Level3AccessRole, ref Lvl3RoleID);
            register(Level4AccessRole, ref Lvl4RoleID);

            static void register(string a, ref ulong b)
            {
                if (!ulong.TryParse(a, out b))
                    b = 0;
            }
        }

        public IReadOnlyList<RegisteredCommand> GetCommands()
        {
            return rawCommands.AsReadOnly();
        }

        private void ReorderCommands()
        {
            orderedCommands.Clear();

            foreach (var command in rawCommands)
                foreach (var alias in command.Attribute.Addressables)
                    orderedCommands.Add(new KeyValuePair<string, RegisteredCommand>(alias, command));

            orderedCommands = orderedCommands.OrderByDescending(c => c.Key.Length).ToList();

            //commands = commands.OrderBy(c => c.Attribute.Aliases.Max(a => a.Length)).ToList();
        }

        private int GetAccessLevel(IGuildUser guildUser)
        {
            var accessLevel = 0;

            foreach (var roleID in guildUser.RoleIds)
            {
                if (roleID == Lvl1RoleID)
                    accessLevel = Math.Max(accessLevel, 1);

                if (roleID == Lvl2RoleID)
                    accessLevel = Math.Max(accessLevel, 2);

                if (roleID == Lvl3RoleID)
                    accessLevel = Math.Max(accessLevel, 3);

                if (roleID == Lvl4RoleID)
                    accessLevel = Math.Max(accessLevel, 4);
            }

            return accessLevel;
        }

        public void SetDirty()
        {
            dirtyCommands = true;
        }

        public struct RegisteredCommand
        {
            public CommandAttribute Attribute;
            public Func<CommandParameters, Task> Delegate;
            public CommandContainerListener Listener;

            public RegisteredCommand(CommandAttribute attribute, Func<CommandParameters, Task> @delegate, CommandContainerListener listener)
            {
                Attribute = attribute;
                Delegate = @delegate;
                Listener = listener;
            }
        }
    }
}
