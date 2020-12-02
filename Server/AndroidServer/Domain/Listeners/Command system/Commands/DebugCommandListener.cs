using Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AndroidServer.Domain.Listeners.Commands
{
    /// <summary>
    /// Debug commands
    /// </summary>
    public class DebugCommandListener : CommandContainerListener
    {
        public DebugCommandListener(AndroidInstance android, ulong channelID) : base(android, channelID) { }

        [Command(CommandAccessLevel.Level1, "ping")]
        public async Task Ping(CommandParameters p)
        {
            var now = DateTime.UtcNow;
            var time = p.SocketMessage.Timestamp.UtcDateTime;
            var delay = now - time;

            await p.Reply($"pong! {Math.Round(delay.TotalMilliseconds, 3)} ms");
        }

        [Command(CommandAccessLevel.Level1, "list", "listeners", "list listeners", "list all listeners")]
        public async Task List(CommandParameters parameters)
        {
            var listeners = parameters.Instance.Listeners;

            var channelListeners = listeners.Where(c => c.GlobalListener || c.ChannelID == parameters.SocketMessage.Channel.Id);

            await parameters.SocketMessage.Channel.SendMessageAsync(string.Join('\n', channelListeners.Select(l => toString(l))));

            static string toString(AndroidListener listener)
            {
                var s = (listener.DisplayName ?? listener.Name) + "[";

                if (listener.GlobalListener)
                    s += "GLOBAL";

                s += $"@{listener.ChannelID}]";

                return s;
            }
        }

        [Command(CommandAccessLevel.Level4, "commands", "list commands", "list all commands")]
        public async Task Commands(CommandParameters parameters)
        {
            var listener = parameters.Instance.GetListener<CommandListener>();

            var role1 = parameters.Instance.Guild.GetRole(listener.Lvl1RoleID);
            var role2 = parameters.Instance.Guild.GetRole(listener.Lvl1RoleID);
            var role3 = parameters.Instance.Guild.GetRole(listener.Lvl1RoleID);
            var role4 = parameters.Instance.Guild.GetRole(listener.Lvl1RoleID);

            if (role1 == null || role2 == null || role3 == null || role4 == null)
                return;

            var commands = listener.GetCommands();
            StringBuilder b = new StringBuilder();

            var level1 = commands.Where(c => c.Attribute.AccessLevel == (int)CommandAccessLevel.Level1);
            var level2 = commands.Where(c => c.Attribute.AccessLevel == (int)CommandAccessLevel.Level2);
            var level3 = commands.Where(c => c.Attribute.AccessLevel == (int)CommandAccessLevel.Level3);
            var level4 = commands.Where(c => c.Attribute.AccessLevel == (int)CommandAccessLevel.Level4);

            appendCommands(CommandAccessLevel.Level1);
            appendCommands(CommandAccessLevel.Level2);
            appendCommands(CommandAccessLevel.Level3);
            appendCommands(CommandAccessLevel.Level4);

            await parameters.SocketMessage.Channel.SendMessageAsync(b.ToString());

            void appendCommands(CommandAccessLevel level)
            {
                IEnumerable<CommandListener.RegisteredCommand> commands = null;
                IRole role = null;

                switch (level)
                {
                    case CommandAccessLevel.Level1:
                        commands = level1;
                        role = role1;
                        break;
                    case CommandAccessLevel.Level2:
                        commands = level2;
                        role = role2;
                        break;
                    case CommandAccessLevel.Level3:
                        commands = level3;
                        role = role3;
                        break;
                    case CommandAccessLevel.Level4:
                        commands = level4;
                        role = role4;
                        break;
                }

                if (level != CommandAccessLevel.Level1)
                    b.Append('\n');

                b.Append("**Access Level ");
                b.Append((int)level);
                b.Append("**\n");
                foreach (var item in commands)
                {
                    b.AppendJoin(", ", item.Attribute.Addressables);
                    b.Append('\n');
                }
            }
        }
    }
}
