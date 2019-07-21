using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace NashorMatch.Discord.Services
{
    public class EventService
    {
        readonly IServiceProvider services;
        readonly DiscordSocketClient client;
        readonly CommandService commandService;
        readonly PermissionService permissionService;
        readonly RoleService roleService;
        readonly ChannelService channelService;
        readonly CoordinatorService coordinatorService;

        public EventService(
            IServiceProvider services,
            DiscordSocketClient client,
            CommandService commandService,
            PermissionService permissionService,
            RoleService roleService,
            ChannelService channelService,
            CoordinatorService coordinatorService)
        {
            this.services = services;
            this.client = client;
            this.commandService = commandService;
            this.permissionService = permissionService;
            this.roleService = roleService;
            this.channelService = channelService;
            this.coordinatorService = coordinatorService;
            client.Disconnected += Disconnected;
            client.GuildMemberUpdated += GuildMemberUpdated;
            client.MessageReceived += MessageReceived;
            client.Ready += Ready;
        }

        Task Disconnected(Exception e)
        {
            Console.WriteLine(e);
            Environment.Exit(-1);
            return Task.CompletedTask;
        }

        async Task GuildMemberUpdated(SocketGuildUser before, SocketGuildUser after)
        {
            if ((before.Username != after.Username) || (before.Nickname != after.Nickname))
                if (after.Roles.Any(x => x.Name == "verified"))
                    await after.RemoveRoleAsync(after.Roles.Where(x => x.Name == "verified").FirstOrDefault());
        }

        async Task MessageReceived(SocketMessage msg)
        {
            var tmp = (SocketUserMessage)msg;
            if (tmp == null) return;
            var pos = 0;
            if (!(tmp.HasCharPrefix('>', ref pos) ||
                tmp.HasMentionPrefix(client.CurrentUser, ref pos)) ||
                tmp.Author.IsBot)
                return;
            var context = new SocketCommandContext(client, tmp);
            var result = await commandService.ExecuteAsync(context, pos, services);
            if (!result.IsSuccess)
                Console.WriteLine(result.ErrorReason);
        }

        async Task Ready()
        {
            //await channelService.GetValues();
            await permissionService.VerifyPermissions();
            await roleService.GenerateRolesAsync();
            await channelService.GenerateChannelsAsync();
            await roleService.StartRoleDelegationTasks();
            await coordinatorService.StartCoordinatorTasks();
        }
    }
}
