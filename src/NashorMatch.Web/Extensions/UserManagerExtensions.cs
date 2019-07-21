using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NashorMatch.Web.Models;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace NashorMatch.Web.Extensions
{
    public static class UserManagerExtensions
    {
        public static async Task<ApplicationUser> GetUserWithRelationsAsync(this UserManager<ApplicationUser> userManager, ClaimsPrincipal principal)
        {
            if (principal == null)
                throw new ArgumentNullException(nameof(principal));
            var id = userManager.GetUserId(principal);
            var user = id == null ? await Task.FromResult<ApplicationUser>(null) : await userManager.FindByIdAsync(id);
            return await userManager.Users.Include(x => x.DiscordUser).Include(x => x.DiscordConnections).SingleAsync(x => x == user);
        }
    }
}
