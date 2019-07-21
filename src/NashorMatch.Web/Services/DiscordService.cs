using Microsoft.AspNetCore.Identity;
using NashorMatch.Web.Models;
using NashorMatch.Web.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace NashorMatch.Web.Services
{
    public class DiscordService : IDiscordService
    {
        HttpClient HttpClient;

        public DiscordService()
        {
            HttpClient = new HttpClient();
        }

        public async Task<ICollection<DiscordConnection>> GetUserConnectionsAsync(UserManager<ApplicationUser> userManager, ApplicationUser user)
        {
            if (await AccessTokenIsExpiredAsync(userManager, user))
                await RenewAccessTokenAsync(userManager, user);
            var token = await userManager.GetAuthenticationTokenAsync(user, "Discord", "access_token");
            HttpClient.DefaultRequestHeaders.Clear();
            HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            HttpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/x-www-form-urlencoded"));
            var response = await HttpClient.GetAsync("https://discordapp.com/api/users/@me/connections");
            return await response.Content.ReadAsAsync<ICollection<DiscordConnection>>();
        }

        public async Task<DiscordUser> GetUserProfileAsync(UserManager<ApplicationUser> userManager, ApplicationUser user)
        {
            if (await AccessTokenIsExpiredAsync(userManager, user))
                await RenewAccessTokenAsync(userManager, user);
            var token = await userManager.GetAuthenticationTokenAsync(user, "Discord", "access_token");
            HttpClient.DefaultRequestHeaders.Clear();
            HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            HttpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/x-www-form-urlencoded"));
            var response = await HttpClient.GetAsync("https://discordapp.com/api/users/@me");
            return await response.Content.ReadAsAsync<DiscordUser>();
        }

        async Task<bool> AccessTokenIsExpiredAsync(UserManager<ApplicationUser> userManager, ApplicationUser user) =>
            DateTime.Parse(await userManager.GetAuthenticationTokenAsync(user, "Discord", "expires_at")).AddDays(-2) < DateTime.Now;

        async Task RenewAccessTokenAsync(UserManager<ApplicationUser> userManager, ApplicationUser user)
        {
            var token = await userManager.GetAuthenticationTokenAsync(user, "Discord", "refresh_token");
            var parameters = new Dictionary<string, string>()
            {
                { "client_id", Environment.GetEnvironmentVariable("AppId")},
                { "client_secret", Environment.GetEnvironmentVariable("AppSecret") },
                { "grant_type", "refresh_token" },
                { "refresh_token", token },
#if DEBUG
                { "redirect_uri", "http://localhost:5001/signin-discord" },
#else
                { "redirect_uri", "http://nashormatch.com/signin-discord" },
#endif
                { "scope", "identify email connections" }
            };
            HttpClient.DefaultRequestHeaders.Clear();
            var response = await HttpClient.PostAsync(new Uri("https://discordapp.com/api/oauth2/token"), new FormUrlEncodedContent(parameters));
            if (response.IsSuccessStatusCode)
            {
                var access = await response.Content.ReadAsAsync<DiscordAccessTokenResponse>();
                await userManager.SetAuthenticationTokenAsync(user, "Discord", "access_token", access.Access_Token);
                await userManager.SetAuthenticationTokenAsync(user, "Discord", "token_type", access.Token_Type);
                await userManager.SetAuthenticationTokenAsync(user, "Discord", "expires_at", DateTime.Now.AddSeconds(access.Expires_In).ToString("yyyy-MM-ddTHH:mm:ss.fffffffK"));
                await userManager.SetAuthenticationTokenAsync(user, "Discord", "refresh_token", access.Refresh_Token);
            }
        }
    }
}
