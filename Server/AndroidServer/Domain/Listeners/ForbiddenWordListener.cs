using Discord.WebSocket;
using System;
using System.Threading.Tasks;

namespace AndroidServer.Domain.Listeners
{
    /// <summary>
    /// Deletes a message that contains specific text
    /// </summary>
    public class ForbiddenWordListener : AndroidListener
    {
        [UiVariableType(VariableType.TextArea)]
        public string ForbiddenText { get; set; }

        [UiVariableType(VariableType.Boolean)]
        public bool CaseSensitive { get; set; }

        public ForbiddenWordListener(AndroidInstance android, ulong channelID) : base(android, channelID) { }

        public override async Task OnMessage(SocketMessage message)
        {
            if (string.IsNullOrWhiteSpace(ForbiddenText))
                return;

            var content = message.Content;
            var violation = ForbiddenText;

            if (!CaseSensitive)
            {
                content = content.ToLower();
                violation = violation.ToLower();
            }

            if (content.Contains(violation))
                await message.DeleteAsync();
        }
    }
}
