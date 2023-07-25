using Discord.WebSocket;

namespace DiscordBot.Bot.Events
{
    public class MessageReceivedFromUserEvent
    {
        public SocketMessage Message { get; set; }
    }
}