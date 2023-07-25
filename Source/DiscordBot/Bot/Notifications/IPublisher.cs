namespace DiscordBot.Bot.Notifications
{
    public interface IPublisher
    {
        Task PublishAsync<TEvent>(TEvent @event);
    }
}