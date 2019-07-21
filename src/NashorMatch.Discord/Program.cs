using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using NashorMatch.Discord.Services;
using System;
using System.Threading.Tasks;

namespace NashorMatch.Discord
{
    class Program
    {
        IServiceProvider services;
        DiscordSocketClient client;

        static void Main(string[] args) =>
            new Program().MainAsync().GetAwaiter().GetResult();

        async Task MainAsync()
        {
            client = new DiscordSocketClient();
            services = ConfigureServices();
            await services.GetRequiredService<RegistrationService>().IntializeRegistrationsAsync();
            await client.LoginAsync(
                TokenType.Bot,
                Environment.GetEnvironmentVariable("NashorMatchDiscordToken"));
            await client.StartAsync();
            await client.SetGameAsync("'>help' for commands");
            await Task.Delay(-1);
        }

        IServiceProvider ConfigureServices()
        {
            return new ServiceCollection()
                .AddSingleton(client)
                .AddSingleton<CommandService>()
                .AddSingleton<RegistrationService>()
                .AddSingleton<PermissionService>()
                .AddSingleton<HttpService>()
                .AddSingleton<RiotService>()
                .AddSingleton<RoleService>()
                .AddSingleton<ChannelService>()
                .AddSingleton<EventService>()
                .AddSingleton<CoordinatorService>()
                .BuildServiceProvider();
        }
    }
}
