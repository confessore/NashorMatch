namespace NashorMatch.Web.Models
{
    public class DiscordAccessTokenResponse
    {
        public string Access_Token { get; set; }
        public string Token_Type { get; set; }
        public double Expires_In { get; set; }
        public string Refresh_Token { get; set; }
        public string Scope { get; set; }
    }
}
