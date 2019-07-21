﻿using Discord;
using Discord.WebSocket;
using NashorMatch.Discord.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NashorMatch.Discord.Services
{
    public class RoleService
    {
        readonly DiscordSocketClient client;
        readonly RiotService riotService;
        readonly HttpService httpService;

        public RoleService(
            DiscordSocketClient client,
            RiotService riotService,
            HttpService httpService)
        {
            this.client = client;
            this.riotService = riotService;
            this.httpService = httpService;
        }

        public async Task GenerateRolesAsync()
        {
            foreach (var guild in client.Guilds)
                foreach (var pair in Roles)
                    if (!guild.Roles.Any(x => x.Name.ToLower() == pair.Key))
                        await guild.CreateRoleAsync(pair.Key, new GuildPermissions(pair.Value.permissions), pair.Value.color, ShouldHoistRole(pair.Key) ? true : false);
        }

        public Task StartRoleDelegationTasks()
        {
            foreach (var guild in client.Guilds)
            {
                _ = Task.Run(async () =>
                {
                    while (true)
                    {
                        foreach (var user in guild.Users.Where(x => !OfflineOrInvisible(x.Status)))
                        {
                            if (!HasBotRole(user))
                            {
                                try
                                {
                                    var tier = riotService.GetBestSoloRank(await riotService.GetLeaguesByNameAsync(riotService.Region(guild.Name), user.Nickname ?? user.Username));
                                    if (!string.IsNullOrEmpty(tier))
                                    {
                                        if (CurrentStandardRole(user) == null || !IsSame(CurrentStandardRole(user), FindGuildRole(guild, tier)))
                                            await UpdateTierRoleAsync(user, guild, tier);
                                        /*if (CurrentStandardRole(user) == null || IsPromotion(CurrentStandardRole(user), GetGuildRole(guild, tier)))
                                        {
                                            await UpdateStandardRoleAsync(user, guild, tier);
                                            await SendPromotionMessageAsync(guild, "general", user, tier);
                                        }
                                        else if (!IsPromotion(CurrentStandardRole(user), GetGuildRole(guild, tier)) && !IsSame(CurrentStandardRole(user), GetGuildRole(guild, tier)))
                                            await UpdateStandardRoleAsync(user, guild, tier);*/
                                    }
                                    else if (CurrentStandardRole(user) == null || !IsSame(CurrentStandardRole(user), FindGuildRole(guild, "unranked")))
                                        await UpdateTierRoleAsync(user, guild, "unranked");
                                }
                                catch { await RemoveTierRolesAsync(user); }
                                finally { await Task.Delay(2500); }
                            }
                        }
                    }
                });
            }
            return Task.CompletedTask;
        }

        async Task SendPromotionMessageAsync(SocketGuild guild, string channel, SocketGuildUser user, string tier) =>
            await guild.TextChannels.Where(x => x.Name.ToLower() == channel.ToLower()).FirstOrDefault()
                .SendMessageAsync($":tada: **{user.Nickname ?? user.Username}** was promoted to :sparkles: **{tier}** :sparkles:");

        async Task UpdateTierRoleAsync(SocketGuildUser user, SocketGuild guild, string name)
        {
            var roles = user.Roles.ToList();
            foreach (var role in user.Roles)
                if (!IsAdminRole(role.Name) && !IsLaneRole(role.Name))
                    roles.Remove(role);
            roles.Add(FindGuildRole(guild, name));
            if (!HasVerifiedRole(user) && await GetVerificationStatus(user))
                roles.Add(FindGuildRole(guild, "verified"));
            await user.ModifyAsync(x => x.Roles = roles);
        }

        public async Task UpdateLaneRoleAsync(SocketGuildUser user, SocketGuild guild, string name)
        {
            var roles = user.Roles.ToList();
            foreach (var role in user.Roles)
                if (!IsAdminRole(role.Name) && !IsTierRole(role.Name))
                    roles.Remove(role);
            roles.Add(FindGuildRole(guild, name));
            await user.ModifyAsync(x => x.Roles = roles);
        }

        async Task RemoveTierRolesAsync(SocketGuildUser user)
        {
            var roles = user.Roles.ToList();
            foreach (var role in user.Roles)
                if (!IsAdminRole(role.Name) && !IsLaneRole(role.Name))
                    roles.Remove(role);
            await user.ModifyAsync(x => x.Roles = roles);
        }

        SocketRole CurrentStandardRole(SocketGuildUser user) =>
            user.Roles.Where(x => x.Name.ToLower() != "@everyone" && !IsAdminRole(x.Name.ToLower()) && !IsLaneRole(x.Name.ToLower())).FirstOrDefault();

        SocketRole FindGuildRole(SocketGuild guild, string name) =>
            guild.Roles.Where(x => x.Name.ToLower() == name.ToLower()).FirstOrDefault();

        bool IsPromotion(SocketRole current, SocketRole proposed) =>
            Enum.Parse<DiscordTierRole>(current.Name.ToLower()) > Enum.Parse<DiscordTierRole>(proposed.Name.ToLower());

        bool IsSame(SocketRole current, SocketRole proposed) =>
            Enum.Parse<DiscordTierRole>(current.Name.ToLower()) == Enum.Parse<DiscordTierRole>(proposed.Name.ToLower());

        bool IsAdminRole(string name) =>
            Enum.GetNames(typeof(DiscordAdminRole)).Any(x => x == name.ToLower());

        bool IsTierRole(string name) =>
            Enum.GetNames(typeof(DiscordTierRole)).Any(x => x == name.ToLower());

        bool IsLaneRole(string name) =>
            Enum.GetNames(typeof(DiscordLaneRole)).Any(x => x == name.ToLower());

        bool HasBotRole(SocketGuildUser user) =>
            user.Roles.Any(x => x.Name.ToLower() == "bot");

        bool OfflineOrInvisible(UserStatus status) =>
            status == UserStatus.Offline || status == UserStatus.Invisible;

        bool ShouldHoistRole(string name) =>
            !IsLaneRole(name.ToLower()) && name.ToLower() != "verified";

        bool HasVerifiedRole(SocketGuildUser user) =>
            user.Roles.Any(x => x.Name.ToLower() == "verified");

        async Task<bool> GetVerificationStatus(SocketGuildUser user)
        {
            var response = await httpService.HttpClient.GetAsync($"https://localhost:44362/Home/Verified?region=euw&id={user.Id}&name={user.Nickname ?? user.Username}");
            return response.IsSuccessStatusCode;
        }

        Dictionary<string, (ulong permissions, Color color)> Roles =>
            new Dictionary<string, (ulong, Color)>()
            {
                { "admin", ((ulong)DiscordRole.admin, Color.Orange) },
                { "developer", ((ulong)DiscordRole.developer, Color.Magenta) },
                { "bot", ((ulong)DiscordRole.bot, Color.DarkRed) },
                { "moderator", ((ulong)DiscordRole.moderator, Color.Blue) },
                { "verified", ((ulong)DiscordRole.verified, Color.Default) },
                { "top", ((ulong)DiscordRole.top, Color.Default) },
                { "mid", ((ulong)DiscordRole.mid, Color.Default) },
                { "jng", ((ulong)DiscordRole.jng, Color.Default) },
                { "adc", ((ulong)DiscordRole.adc, Color.Default) },
                { "sup", ((ulong)DiscordRole.sup, Color.Default) },
                { "challenger", ((ulong)DiscordRole.challenger, Color.Green) },
                { "grandmaster", ((ulong)DiscordRole.grandmaster, Color.Green) },
                { "master", ((ulong)DiscordRole.master, Color.Green) },
                { "diamond", ((ulong)DiscordRole.diamond, Color.DarkTeal) },
                { "platinum", ((ulong)DiscordRole.platinum, Color.LighterGrey) },
                { "gold", ((ulong)DiscordRole.gold, Color.Gold) },
                { "silver", ((ulong)DiscordRole.silver, Color.LightGrey) },
                { "bronze", ((ulong)DiscordRole.bronze, Color.DarkGrey) },
                { "iron", ((ulong)DiscordRole.iron, Color.DarkerGrey) },
                { "unranked", ((ulong)DiscordRole.unranked, Color.DarkerGrey) },
            };
    }
}