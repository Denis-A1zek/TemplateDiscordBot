using DiscordBot.Bot;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

Console.WriteLine("Hello, World!");

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);
builder.Configuration.AddJsonFile("appsettings.json");

//DI Container
builder.Services.AddBot();
builder.Services.AddPublisher();

using IHost host = builder.Build();

var bot = new DiscordBot.Bot.DiscordBot(host.Services);

await bot.UseSlashCommands();
bot.UseMessageReceiver();

bot.UseButtonHandlers();
bot.UseLoggingService();

await bot.Run();

await host.RunAsync();

