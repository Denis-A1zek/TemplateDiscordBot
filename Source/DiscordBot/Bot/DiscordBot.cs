using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using DiscordBot.Bot.Events;
using DiscordBot.Bot.Notifications;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Reflection;

namespace DiscordBot.Bot
{
    internal class DiscordBot
    {
        private readonly IServiceProvider _serviceProvider;

        public DiscordBot(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            Client = _serviceProvider.GetRequiredService<DiscordSocketClient>();
        }

        public DiscordSocketClient Client { get; private set; }
        public InteractionService InteractionService { get; private set; }

        public async Task UseSlashCommands()
        {
            InteractionService = _serviceProvider.GetRequiredService<InteractionService>();
            await InteractionService.AddModulesAsync(Assembly.GetExecutingAssembly(), _serviceProvider);
            Client.InteractionCreated += HandleInteraction;
            InteractionService.SlashCommandExecuted += SlashCommandExecuted;
        }

        public void UseMessageReceiver()
        {
            Client.MessageReceived += MessageReceiverCommandHandler;
        }

        public void UseButtonHandlers()
        {
            Client.ButtonExecuted += ButtonEventClickHandler;
        }


        public void UseLoggingService()
        {
            Client.Log += LogAsync;
        }

        private Task LogAsync(LogMessage message)
        {
            if (message.Exception is CommandException cmdException)
            {
                Console.WriteLine($"[Command/{message.Severity}] {cmdException.Command.Aliases.First()}"
                    + $" failed to execute in {cmdException.Context.Channel}.");
                Console.WriteLine(cmdException);
            }
            else
                Console.WriteLine($"[General/{message.Severity}] {message}");

            return Task.CompletedTask;
        }

        public async Task Run()
        {
            Client.Ready += ReadyAsync;
            var configuration = _serviceProvider.GetRequiredService<IConfiguration>();
            await Client.LoginAsync(TokenType.Bot, configuration["Token"]);
            await Client.StartAsync();
        }

        async Task ReadyAsync()
        {
            var guildId = _serviceProvider.GetRequiredService<IConfiguration>()["GuildId"];
            var logger = _serviceProvider.GetRequiredService<ILogger<DiscordBot>>();
#if DEBUG
            logger.LogInformation($"In debug mode, adding commands to {guildId}...");
            await InteractionService.RegisterCommandsToGuildAsync(ulong.Parse(guildId));
#else
            await InteractionService.RegisterCommandsGloballyAsync(true);
#endif
            logger.LogInformation($"Connected as -> [{Client.CurrentUser}] :)");
        }

        private async Task HandleInteraction(SocketInteraction arg)
        {
            try
            {
                var ctx = new SocketInteractionContext(Client, arg);
                await InteractionService.ExecuteCommandAsync(ctx, _serviceProvider);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                if (arg.Type == InteractionType.ApplicationCommand)
                {
                    await arg.GetOriginalResponseAsync().ContinueWith(async (msg) => await msg.Result.DeleteAsync());
                }
            }
        }

        private async Task SlashCommandExecuted
            (SlashCommandInfo info, IInteractionContext context, Discord.Interactions.IResult result)
        {
            if (!result.IsSuccess)
            {
                switch (result.Error)
                {
                    case InteractionCommandError.UnknownCommand:
                        break;
                    case InteractionCommandError.ConvertFailed:
                        break;
                    case InteractionCommandError.BadArgs:
                        break;
                    case InteractionCommandError.Exception:
                        break;
                    case InteractionCommandError.Unsuccessful:
                        break;
                    case InteractionCommandError.UnmetPrecondition:
                        break;
                    case InteractionCommandError.ParseFailed:
                        break;
                    case null:
                        break;
                    default:
                        break;
                }
            }
            await Task.CompletedTask;
        }

        public Task ButtonEventClickHandler(SocketMessageComponent messageComponent)
        {
            _ = Task.Run(async () =>
            {
                //Логика обработки кнопок по id
            });
            return Task.CompletedTask;
        }

        private async Task MessageReceiverCommandHandler(SocketMessage message)
        {
            _ = Task.Run(async () =>
            {
                var publisher = _serviceProvider.GetRequiredService<IPublisher>();
                var configuration = _serviceProvider.GetRequiredService<IConfiguration>();
                ulong guildId = (message.Channel as SocketGuildChannel)?.Guild?.Id ?? 0;
                var isEq = guildId == ulong.Parse(configuration["GuildId"]);

                if (!message.Author.IsBot && isEq)
                {
                    await publisher.PublishAsync(new MessageReceivedFromUserEvent() { Message = message });
                }
            });
            await Task.CompletedTask;
        }

    }
}