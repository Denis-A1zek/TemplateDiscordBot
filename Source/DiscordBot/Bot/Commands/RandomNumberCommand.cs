using Discord.Interactions;

namespace DiscordBot.Bot.Commands
{
    public class RandomNumberCommand : Command
    {
        private Random _random = new();

        [SlashCommand("random", "Randomizer from 0 to 9")]
        public async Task Randomize()
        {
            await RespondAsync($"Выпала цифра {_random.Next(0, 9)}");
        }
    }
}