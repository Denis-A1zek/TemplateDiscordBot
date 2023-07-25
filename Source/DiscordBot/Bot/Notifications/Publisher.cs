using Microsoft.Extensions.DependencyInjection;

namespace DiscordBot.Bot.Notifications
{
    public class Publisher : IPublisher
    {
        private readonly Dictionary<Type, List<object>> _subscribersAsyncMap = new();

        private readonly IServiceProvider _serviceProvider;

        public Publisher(IServiceProvider provider)
        {
            _serviceProvider = provider;
        }

        public void Subscribe(object handler, Type type)
        {
            if (!_subscribersAsyncMap.ContainsKey(type))
            {
                _subscribersAsyncMap.Add(type, new List<object>());
                _subscribersAsyncMap[type].Add(handler);
                return;
            }
            _subscribersAsyncMap[type].Add(handler);
        }

        public void Unsubscribe<TEvent>(IAsyncEventHandler<TEvent> handler)
        {
            _subscribersAsyncMap.Remove(typeof(TEvent));
        }

        public async Task PublishAsync<TEvent>(TEvent @event)
        {
            var type = typeof(IEnumerable<IAsyncEventHandler<TEvent>>);
            var subscribers = _serviceProvider.GetRequiredService(type);
            foreach (var item in ((IAsyncEventHandler<TEvent>[])subscribers))
            {
                await item.HandleAsync(@event);
            }
        }
    }
}