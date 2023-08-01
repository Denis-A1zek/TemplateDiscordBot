using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBot.Bot.Settings;

public class BotSettings
{
    public string Token { get; set; }
    public string GuildId { get; set; }
    private string BotId { get; set; }
}
