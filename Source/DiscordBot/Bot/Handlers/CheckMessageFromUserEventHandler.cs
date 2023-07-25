
using DiscordBot.Bot.Events;
using DiscordBot.Bot.Notifications;

namespace DiscordBot.Bot.Handlers
{
    public class CheckMessageFromUserEventHandler : IAsyncEventHandler<MessageReceivedFromUserEvent>
    {

        public async Task HandleAsync(MessageReceivedFromUserEvent @event)
        {
            await Console.Out.WriteLineAsync($"Написал");
        }

    }
}