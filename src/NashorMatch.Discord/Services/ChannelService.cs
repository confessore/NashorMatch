using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NashorMatch.Discord.Services
{
    public class ChannelService
    {
        readonly DiscordSocketClient client;

        public ChannelService(DiscordSocketClient client)
        {
            this.client = client;
        }

        public async Task GenerateChannelsAsync()
        {
            await GenerateTextChannelsAsync();
            await GenerateVoiceChannelsAsync();
            await StartTextChannelOverwritePermissionsTask();
        }

        async Task GenerateTextChannelsAsync()
        {
            await RemoveCategoryChannelsAsync();
            foreach (var guild in client.Guilds)
            {
                foreach (var pair in TextChannels)
                {
                    if (!TextChannelExists(guild, pair.Key))
                    {
                        var tmp = await guild.CreateTextChannelAsync(pair.Key);
                        foreach (var role in guild.Roles)
                        {
                            if (role.IsEveryone)
                                await tmp.AddPermissionOverwriteAsync(role, pair.Value.everyone);
                            else
                                await tmp.AddPermissionOverwriteAsync(role, pair.Value.member);
                        }
                    }
                }
            }
        }

        async Task GenerateVoiceChannelsAsync()
        {
            await RemoveCategoryChannelsAsync();
            foreach (var guild in client.Guilds)
            {
                foreach (var pair in VoiceChannels)
                {
                    if (!VoiceChannelExists(guild, pair.Key))
                    {
                        var tmp = await guild.CreateVoiceChannelAsync(pair.Key);
                        foreach (var role in guild.Roles)
                        {
                            if (role.IsEveryone)
                                await tmp.AddPermissionOverwriteAsync(role, pair.Value.everyone);
                            else
                                await tmp.AddPermissionOverwriteAsync(role, pair.Value.member);
                        }
                    }
                }
            }
        }

        Task StartTextChannelOverwritePermissionsTask()
        {
            _ = Task.Run(async () =>
            {
                await RemoveCategoryChannelsAsync();
                foreach (var guild in client.Guilds)
                {
                    foreach (var pair in TextChannels)
                    {
                        if (TextChannelExists(guild, pair.Key))
                        {
                            var tmp = GetTextChannel(guild, pair.Key);
                            foreach (var role in guild.Roles)
                            {
                                if (role.IsEveryone)
                                {
                                    if (!tmp.GetPermissionOverwrite(role).Equals(pair.Value.everyone))
                                        await tmp.AddPermissionOverwriteAsync(role, pair.Value.everyone);
                                }
                                else
                                {
                                    if (!tmp.GetPermissionOverwrite(role).Equals(pair.Value.member))
                                        await tmp.AddPermissionOverwriteAsync(role, pair.Value.member);
                                }
                            }
                        }
                    }
                }
            });
            return Task.CompletedTask;
        }

        async Task RemoveCategoryChannelsAsync()
        {
            foreach (var guild in client.Guilds)
                foreach (var cc in guild.CategoryChannels)
                    await cc.DeleteAsync();
        }
        
        SocketTextChannel GetTextChannel(SocketGuild guild, string channel) =>
            guild.TextChannels.Where(x => x.Name.ToLower() == channel.ToLower()).FirstOrDefault();

        SocketVoiceChannel GetVoiceChannel(SocketGuild guild, string channel) =>
            guild.VoiceChannels.Where(x => x.Name.ToLower() == channel.ToLower()).FirstOrDefault();

        bool TextChannelExists(SocketGuild guild, string channel) =>
            GetTextChannel(guild, channel) != null;

        bool VoiceChannelExists(SocketGuild guild, string channel) =>
            GetVoiceChannel(guild, channel) != null;

        public Task GetValues()
        {
            foreach (var guild in client.Guilds)
                foreach (var channel in guild.TextChannels)
                    foreach (var ow in channel.PermissionOverwrites)
                        Console.WriteLine($"{guild}    {channel}    {ow}    {ow.Permissions}    {ow.Permissions.AllowValue}    {ow.Permissions.DenyValue}");
            return Task.CompletedTask;
        }

        static Dictionary<string, (OverwritePermissions everyone, OverwritePermissions member)> TextChannels =>
            new Dictionary<string, (OverwritePermissions, OverwritePermissions)>()
            {
                { "welcome", (new OverwritePermissions(66560, 2048), new OverwritePermissions(0, 1024)) },
                { "guides", (new OverwritePermissions(0, 1024), new OverwritePermissions(1024, 0)) },
                { "lfg", (new OverwritePermissions(0, 1024), new OverwritePermissions(1024, 0)) },
                { "general", (new OverwritePermissions(0, 1024), new OverwritePermissions(1024, 0)) },
                { "offtopic", (new OverwritePermissions(0, 1024), new OverwritePermissions(1024, 0)) },
                { "help", (new OverwritePermissions(0, 1024), new OverwritePermissions(1024, 2048)) }
            };

        static Dictionary<string, (OverwritePermissions everyone, OverwritePermissions member)> VoiceChannels =>
            new Dictionary<string, (OverwritePermissions, OverwritePermissions)>()
            {
                { "General", (new OverwritePermissions(0, 1024), new OverwritePermissions(1024, 0)) },
                { "DuoQ-1", (new OverwritePermissions(0, 1024), new OverwritePermissions(1024, 0)) },
                { "DuoQ-2", (new OverwritePermissions(0, 1024), new OverwritePermissions(1024, 0)) },
                { "DuoQ-3", (new OverwritePermissions(0, 1024), new OverwritePermissions(1024, 0)) },
                { "FlexQ-1", (new OverwritePermissions(0, 1024), new OverwritePermissions(1024, 0)) },
                { "FlexQ-2", (new OverwritePermissions(0, 1024), new OverwritePermissions(1024, 0)) },
                { "Music", (new OverwritePermissions(0, 1024), new OverwritePermissions(1024, 0)) }
            };
    }
}
