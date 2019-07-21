using Discord;
using Discord.WebSocket;
using System;
using System.Threading.Tasks;

namespace NashorMatch.Discord.Services
{
    public class PermissionService
    {
        readonly DiscordSocketClient client;

        public PermissionService(DiscordSocketClient client)
        {
            this.client = client;
        }

        public async Task VerifyPermissions()
        {
            foreach (var guild in client.Guilds)
            {
                if (!guild.CurrentUser.GuildPermissions.Administrator)
                {
                    Console.WriteLine($"please grant administrator permissions in {guild.Name}");
                    Environment.Exit(-1);
                }
                if (guild.DefaultMessageNotifications != DefaultMessageNotifications.MentionsOnly)
                    await guild.ModifyAsync(x => x.DefaultMessageNotifications = DefaultMessageNotifications.MentionsOnly);
            }
        }
    }
}
