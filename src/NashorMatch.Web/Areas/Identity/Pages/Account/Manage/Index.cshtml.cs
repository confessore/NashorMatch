using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NashorMatch.Web.Extensions;
using NashorMatch.Web.Models;
using NashorMatch.Web.Services.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NashorMatch.Web.Areas.Identity.Pages.Account.Manage
{
    public partial class IndexModel : PageModel
    {
        readonly UserManager<ApplicationUser> userManager;
        readonly IDiscordService discordService;

        public IndexModel(
            UserManager<ApplicationUser> userManager,
            IDiscordService discordService)
        {
            this.userManager = userManager;
            this.discordService = discordService;
        }

        public string Username { get; set; }
        public string Email { get; set; }
        public ICollection<DiscordConnection> DiscordConnections { get; set; }

        [TempData]
        public string StatusMessage { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await userManager.GetUserWithRelationsAsync(User);
            if (user == null)
                return NotFound($"Unable to load user with ID '{userManager.GetUserId(User)}'.");
            user.DiscordUser = await discordService.GetUserProfileAsync(userManager, user);
            user.DiscordConnections = await discordService.GetUserConnectionsAsync(userManager, user);
            await userManager.UpdateAsync(user);
            Username = user.UserName;
            Email = user.Email;
            DiscordConnections = user.DiscordConnections;
            return Page();
        }
    }
}
