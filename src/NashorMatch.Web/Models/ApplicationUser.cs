using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;

namespace NashorMatch.Web.Models
{
    public class ApplicationUser : IdentityUser
    {
        public virtual DiscordUser DiscordUser { get; set; }
        public virtual ICollection<DiscordConnection> DiscordConnections { get; set; }
    }
}
