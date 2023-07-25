using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using DiscordBot.Bot.Notifications;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBot.Bot
{
    internal static class Dependencies
    {
        public static IServiceCollection AddBot(this IServiceCollection services)
        {
            services.AddSingleton<DiscordSocketClient>(x => new DiscordSocketClient(new DiscordSocketConfig
            {
                HandlerTimeout = 15,
                GatewayIntents = GatewayIntents.All
            }));
            services.AddSingleton(s => new InteractionService(s.GetRequiredService<DiscordSocketClient>()));
            return services;
        }

        public static IServiceCollection AddPublisher(this IServiceCollection services)
        {
            var assemblyTypes = Assembly.GetExecutingAssembly().GetTypes()
           .Where(type => type.GetInterfaces()
               .Any(i => i.IsGenericType &&
                           (i.GetGenericTypeDefinition() == typeof(IAsyncEventHandler<>))));

            foreach (var implmentationEvent in assemblyTypes)
            {
                var publishEvent = implmentationEvent.GetInterfaces().FirstOrDefault();
                services.AddTransient(publishEvent, implmentationEvent);
            }

            services.AddTransient<IPublisher>(s => new Publisher(s));
            return services;
        }
    }
}