namespace DiscordBot.Bot.Notifications
{
    public interface IAsyncEventHandler<in TEvent>
    {
        Task HandleAsync(TEvent @event);
    }
}