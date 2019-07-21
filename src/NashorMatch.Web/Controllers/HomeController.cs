using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NashorMatch.Web.Extensions;
using NashorMatch.Web.Models;
using NashorMatch.Web.ViewModels;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace NashorMatch.Web.Controllers
{
    public class HomeController : Controller
    {
        UserManager<ApplicationUser> userManager;

        public HomeController(UserManager<ApplicationUser> userManager)
        {
            this.userManager = userManager;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        [Route("Verified")]
        public async Task<IActionResult> Verified(string region, long id, string name)
        {
            if (!Request.IsLocal())
                return BadRequest();
            var verified = await userManager.Users.AnyAsync(x =>
                x.DiscordUser.Id == id && x.DiscordConnections.Any(y =>
                    y.Id.ToLower().Contains(region.ToLower()) && y.Type == "leagueoflegends" && y.Name.ToLower() == name.ToLower()));
            if (!verified)
                return BadRequest();
            return Ok();
        }

        [HttpGet]
        public JObject TestAuth()
        {
            if (!Request.IsLocal())
                return JObject.FromObject(new BadRequestResult());
            return JObject.FromObject(new DiscordUser()
            {
                Avatar = "0",
                Discriminator = 1875,
                Flags = "0",
                Id = 0,
                Locale = "0",
                MFA_Enabled = false,
                Username = "krycess"
            });
        }

        [HttpGet]
        public JObject TestUser()
        {
            return JObject.FromObject(new DiscordUser()
            {
                Avatar = "0",
                Discriminator = 1875,
                Flags = "0",
                Id = 0,
                Locale = "0",
                MFA_Enabled = false,
                Username = "krycess"
            });
        }

        public async Task<TimeSpan> Test()
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            var response = await client.GetAsync("https://localhost:44362/Home/TestUser");
            var sw = Stopwatch.StartNew();
            for (int i = 0; i < 1000000; i++)
                await response.Content.ReadAsAsync<DiscordUser>();
            sw.Stop();
            return sw.Elapsed;
        }

        public async Task<TimeSpan> TestNewton()
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            var response = await client.GetAsync("https://localhost:44362/Home/TestUser");
            var sw = Stopwatch.StartNew();
            for (int i = 0; i < 1000000; i++)
                JsonConvert.DeserializeObject<DiscordUser>(await response.Content.ReadAsStringAsync());
            sw.Stop();
            return sw.Elapsed;
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
