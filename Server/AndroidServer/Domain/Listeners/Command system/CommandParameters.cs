using Discord;
using Discord.WebSocket;
using System.Threading.Tasks;

namespace AndroidServer.Domain.Listeners.Commands
{
    public struct CommandParameters
    {
        public SocketMessage SocketMessage;
        public IGuildUser GuildUser;
        public int AccessLevel;
        public string ContentWithoutTrigger;
        public string ContentWithoutTriggerAndCommand;
        public string[] GivenArguments;

        public AndroidInstance Instance;

        public readonly async Task Reply(string message)
        {
            await SocketMessage.Channel.SendMessageAsync(message);
        }
    }
}
