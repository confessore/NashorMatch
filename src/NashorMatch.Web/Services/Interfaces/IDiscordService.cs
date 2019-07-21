using Microsoft.AspNetCore.Identity;
using NashorMatch.Web.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NashorMatch.Web.Services.Interfaces
{
    public interface IDiscordService
    {
        Task<ICollection<DiscordConnection>> GetUserConnectionsAsync(UserManager<ApplicationUser> userManager, ApplicationUser user);
        Task<DiscordUser> GetUserProfileAsync(UserManager<ApplicationUser> userManager, ApplicationUser user);
    }
}
