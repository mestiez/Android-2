using Discord.WebSocket;
using System;
using System.Threading.Tasks;

namespace AndroidServer.Domain.Listeners
{
    /// <summary>
    /// Only allows Steam Workshop links in the channel
    /// </summary>
    public class WorkshopLinkListener : AndroidListener
    {
        [UiVariableType(VariableType.String)]
        public string WorkshopPathStart { get; set; } = "https://steamcommunity.com/sharedfiles/filedetails/?id=";

        public WorkshopLinkListener(AndroidInstance android, ulong channelID) : base(android, channelID) { }

        public override async Task OnMessage(SocketMessage message)
        {
            if (string.IsNullOrWhiteSpace(message.Content))
                await message.Channel.DeleteMessageAsync(message);
            else
            {
                var content = message.Content.Split("\n ".ToCharArray());
                bool isValidWorkshopLink = IsValidSteamWorkshopLink(content[0]);
                if (!isValidWorkshopLink)
                    await message.Channel.DeleteMessageAsync(message);
            }
        }

        private bool IsValidSteamWorkshopLink(string url)
        {
            bool isValidUrl = Uri.IsWellFormedUriString(url, UriKind.Absolute);
            bool isToSteamWorkshop = url.StartsWith(WorkshopPathStart) && url.LastIndexOf(WorkshopPathStart) == 0;
            return isValidUrl && isToSteamWorkshop;
        }
    }
}
