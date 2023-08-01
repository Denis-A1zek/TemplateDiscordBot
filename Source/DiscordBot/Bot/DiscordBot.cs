using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using DiscordBot.Bot.Events;
using DiscordBot.Bot.Notifications;
using DiscordBot.Bot.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Reflection;

namespace DiscordBot.Bot;

internal class DiscordBot
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DiscordBot> _logger;
    private readonly BotSettings _botSettings;

    private Dictionary<string, Func<SocketMessageComponent, Task>> _routingButtons = new();

    public DiscordBot(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _botSettings = _serviceProvider.GetRequiredService<IConfiguration>().GetSection("BotSettings").Get<BotSettings>();
        _logger = _serviceProvider.GetRequiredService<ILogger<DiscordBot>>();
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
            _logger.LogInformation($"[Command/{message.Severity}] {cmdException.Command.Aliases.First()}"
                + $" failed to execute in {cmdException.Context.Channel}.");
            _logger.LogInformation(cmdException.Message);
        }
        else
            _logger.LogInformation($"[General/{message.Severity}] {message}");

        return Task.CompletedTask;
    }

    public async Task Run()
    {
        Client.Ready += ReadyAsync;
        await Client.LoginAsync(TokenType.Bot, _botSettings.Token);
        await Client.StartAsync();
    }

    async Task ReadyAsync()
    {
        var guildId = ulong.Parse(_botSettings.GuildId);       
#if DEBUG
        _logger.LogInformation($"In debug mode, adding commands to {guildId}...");
        await InteractionService.RegisterCommandsToGuildAsync(guildId);
#else
        await InteractionService.RegisterCommandsGloballyAsync(true);
#endif
        _logger.LogInformation($"Connected as -> [{Client.CurrentUser}] :)");
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
            _logger.LogError($"[Exception] {ex.Message}");
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
                    _logger.LogError(result.ErrorReason);
                    break;
                case InteractionCommandError.ConvertFailed:
                    _logger.LogError(result.ErrorReason);
                    break;
                case InteractionCommandError.BadArgs:
                    _logger.LogError(result.ErrorReason);
                    break;
                case InteractionCommandError.Exception:
                    _logger.LogError(result.ErrorReason);
                    break;
                case InteractionCommandError.Unsuccessful:
                    _logger.LogError(result.ErrorReason);
                    break;
                case InteractionCommandError.UnmetPrecondition:
                    _logger.LogError(result.ErrorReason);
                    break;
                case InteractionCommandError.ParseFailed:
                    _logger.LogError(result.ErrorReason);
                    break;
                case null:
                    break;
                default:
                    break;
            }
        }
        await Task.CompletedTask;
    }

    public void MapButton(string route, Func<SocketMessageComponent, Task> eventHandler)
    {
        if (!_routingButtons.ContainsKey(route))
            _routingButtons.Add(route, eventHandler);
    }

    public Task ButtonEventClickHandler(SocketMessageComponent messageComponent)
    {
        _ = Task.Run(async () =>
        {
            var buttonId = messageComponent.Data.CustomId;

            if (_routingButtons.TryGetValue(buttonId, out var routingButtons))
            {
                _logger.LogInformation
                ($"[Command Handler] Была нажата кнопка " +
                $"{buttonId}. Вызов обработчика. Пользователь " +
                $"{messageComponent.User.Id}");

                await routingButtons.Invoke(messageComponent);
            }
            else
            {
                await messageComponent.RespondAsync("Произошла ошибка при обработке", ephemeral: true);
            }
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
            var isEq = guildId == ulong.Parse(_botSettings.GuildId);

            if (!message.Author.IsBot && isEq)
            {
                await publisher.PublishAsync(new MessageReceivedFromUserEvent() { Message = message });
            }
        });
        await Task.CompletedTask;
    }

}