using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NashorMatch.Web.Models
{
    public class DiscordConnection
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public bool Revoked { get; set; }
        public bool Verified { get; set; }
        public bool Friend_Sync { get; set; }
        public bool Show_Activity { get; set; }
        public int Visibility { get; set; }

        [Key]
        public string ApplicationUserId { get; set; }
        [ForeignKey("ApplicationUserId")]
        public virtual ApplicationUser ApplicationUser { get; set; }
    }
}
