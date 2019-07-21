using Discord;
using Discord.Commands;
using Discord.WebSocket;
using NashorMatch.Discord.Services;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace NashorMatch.Discord.Modules
{
    public class CommandModule : ModuleBase<SocketCommandContext>
    {
        readonly IServiceProvider services;
        readonly DiscordSocketClient client;
        readonly CommandService commands;
        readonly RiotService riotService;
        readonly RoleService roleService;
        readonly CoordinatorService coordinatorService;

        public CommandModule(
            IServiceProvider services,
            DiscordSocketClient client,
            CommandService commands,
            RiotService riotService,
            RoleService roleService,
            CoordinatorService coordinatorService)
        {
            this.services = services;
            this.client = client;
            this.commands = commands;
            this.riotService = riotService;
            this.roleService = roleService;
            this.coordinatorService = coordinatorService;
        }

        [Command("verify")]
        [Summary("verified: adds the 'verified' role to a user")]
        async Task VerifyAsync([Remainder] string name)
        {
            await RemoveCommandMessageAsync();
            if (IsVerified)
                await GetGuildUser(name).AddRoleAsync(GetGuildRole("verified"));
            else
                await ReplyAsync("you don't have permission to do that");
        }

        [Command("top")]
        [Summary("verified: adds the 'top' role")]
        async Task TopAsync()
        {
            await RemoveCommandMessageAsync();
            if (IsVerified)
                await roleService.UpdateLaneRoleAsync(SocketGuildUser, SocketGuild, "top");
            else
                await ReplyAsync("you don't have permission to do that");
        }

        [Command("mid")]
        [Summary("verified: adds the 'mid' role")]
        async Task MidAsync()
        {
            await RemoveCommandMessageAsync();
            if (IsVerified)
                await roleService.UpdateLaneRoleAsync(SocketGuildUser, SocketGuild, "mid");
            else
                await ReplyAsync("you don't have permission to do that");
        }

        [Command("jng")]
        [Summary("verified: adds the 'jng' role")]
        async Task JngAsync()
        {
            await RemoveCommandMessageAsync();
            if (IsVerified)
                await roleService.UpdateLaneRoleAsync(SocketGuildUser, SocketGuild, "jng");
            else
                await ReplyAsync("you don't have permission to do that");
        }

        [Command("adc")]
        [Summary("verified: adds the 'adc' role")]
        async Task AdcAsync()
        {
            await RemoveCommandMessageAsync();
            if (IsVerified)
                await roleService.UpdateLaneRoleAsync(SocketGuildUser, SocketGuild, "adc");
            else
                await ReplyAsync("you don't have permission to do that");
        }

        [Command("sup")]
        [Summary("verified: adds the 'sup' role")]
        async Task SupAsync()
        {
            await RemoveCommandMessageAsync();
            if (IsVerified)
                await roleService.UpdateLaneRoleAsync(SocketGuildUser, SocketGuild, "sup");
            else
                await ReplyAsync("you don't have permission to do that");
        }

        [Command("join")]
        [Summary("verified: adds the user to the match making coordinator")]
        async Task JoinAsync([Remainder] string queue)
        {
            await RemoveCommandMessageAsync();
            if (IsVerified)
            {
                if (!coordinatorService.HasLane(SocketGuildUser))
                {
                    await ReplyAsync("please set your lane role before trying to join the queue");
                    return;
                }
                if (queue == "dq")
                {
                    await coordinatorService.AddToDuoQueue(SocketGuildUser, true);
                    await ReplyAsync($"{SocketGuildUser.Nickname ?? SocketGuildUser.Username} joined the {coordinatorService.GetTier(SocketGuildUser).Name.ToUpper()} duo queue as {coordinatorService.GetLane(SocketGuildUser).Name.ToUpper()}");
                }
                else if (queue == "fq")
                {
                    await coordinatorService.AddToFlexQueue(SocketGuildUser, true);
                    await ReplyAsync($"{SocketGuildUser.Nickname ?? SocketGuildUser.Username} joined the {coordinatorService.GetTier(SocketGuildUser).Name.ToUpper()} flex queue as {coordinatorService.GetLane(SocketGuildUser).Name.ToUpper()}");
                }
                else if (queue == string.Empty)
                    await ReplyAsync("please use 'dq' for duo queue and 'fq' for flex queue");
                else
                    await ReplyAsync("you have entered an invalid parameter. please use 'dq' for duo queue and 'fq' for flex queue");
            }
            else
                await ReplyAsync("you don't have permission to do that");
        }

        [Command("leave")]
        [Summary("verified: removes the user from the match making coordinator")]
        async Task LeaveAsync()
        {
            await RemoveCommandMessageAsync();
            if (IsVerified)
            {
                await coordinatorService.AddToDuoQueue(SocketGuildUser, false);
                await coordinatorService.AddToFlexQueue(SocketGuildUser, false);
            }
            else
                await ReplyAsync("you don't have permission to do that");
        }

        [Command("count")]
        [Summary("verified: counts players in coordinator")]
        async Task CountAsync()
        {
            await RemoveCommandMessageAsync();
            if (IsVerified)
                await ReplyAsync($"there are currently {coordinatorService.DuoCount} players queued in the coordinator");
            else
                await ReplyAsync("you don't have permission to do that");
        }

        [Command("find")]
        [Summary("all: searches for leagues by summoner name")]
        async Task FindAsync([Remainder] string name)
        {
            await RemoveCommandMessageAsync();
            try
            {
                var leagues = await riotService.GetLeaguesByNameAsync(riotService.Region(Context.Guild.Name), name);
                if (leagues.Length == 0)
                    await ReplyAsync("no leagues found");
                else
                    foreach (var league in leagues)
                        await ReplyAsync(league.Tier);
            }
            catch { await ReplyAsync("no summoner found"); }
        }

        [Command("help")]
        [Summary("all: displays available commands")]
        async Task HelpAsync()
        {
            await RemoveCommandMessageAsync();
            var embedBuilder = new EmbedBuilder();
            foreach (var command in await commands.GetExecutableCommandsAsync(Context, services))
                embedBuilder.AddField(command.Name, command.Summary ?? "no summary available");
            await ReplyAsync("here's a list of commands and their summaries: ", false, embedBuilder.Build());
        }

        [Command("insult")]
        [Summary("all: got 'em")]
        async Task InsultAsync()
        {
            await RemoveCommandMessageAsync();
            await ReplyAsync("your mother");
        }

        [Command("nick")]
        [Summary("all: change your nick")]
        async Task NickAsync([Remainder] string name)
        {
            await RemoveCommandMessageAsync();
            await client.GetGuild(Context.Guild.Id).GetUser(Context.User.Id).ModifyAsync(x => x.Nickname = name);
        }

        async Task RemoveCommandMessageAsync() =>
            await client.GetGuild(Context.Guild.Id).GetTextChannel(Context.Message.Channel.Id).DeleteMessageAsync(Context.Message);

        SocketRole GetGuildRole(string name) =>
            client.GetGuild(Context.Guild.Id).Roles.Where(x => x.Name.ToLower() == name.ToLower()).FirstOrDefault();

        SocketGuildUser GetGuildUser(string name) =>
            client.GetGuild(Context.Guild.Id).Users.Where(x => (x.Nickname ?? x.Username).ToLower() == name.ToLower()).FirstOrDefault();

        SocketGuild SocketGuild =>
            client.GetGuild(Context.Guild.Id);

        SocketGuildUser SocketGuildUser =>
            client.GetGuild(Context.Guild.Id).GetUser(Context.User.Id);

        bool IsVerified =>
            client.GetGuild(Context.Guild.Id).GetUser(Context.User.Id).Roles.Any(x => x.Name.ToLower().Contains("verified"));
    }
}
