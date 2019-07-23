using NashorMatch.Enums;
using NashorMatch.Models;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace NashorMatch.Discord.Services
{
    public class RiotService
    {
        readonly HttpService httpService;
        readonly string key;

        public RiotService(HttpService httpService)
        {
            this.httpService = httpService;
            key = Environment.GetEnvironmentVariable("NashorMatchRiotKey");
        }

        readonly string protocol = "https://";
        readonly string address = ".api.riotgames.com/";
        readonly string summonersByName = "lol/summoner/v4/summoners/by-name/";
        readonly string leaguesBySummoner = "lol/league/v4/entries/by-summoner/";
        readonly string api = "?api_key=";

        async Task<Summoner> GetSummonerByNameAsync(RiotRegion region, string name)
        {
            var response = await httpService.HttpClient.GetAsync(protocol + region + address + summonersByName + name + api + key);
            if (response.IsSuccessStatusCode)
                return JsonConvert.DeserializeObject<Summoner>(await response.Content.ReadAsStringAsync());
            else
                return new Summoner();
        }

        async Task<League[]> GetLeaguesBySummonerAsync(RiotRegion region, Summoner summoner)
        {
            var response = await httpService.HttpClient.GetAsync(protocol + region + address + leaguesBySummoner + summoner.Id + api + key);
            if (response.IsSuccessStatusCode)
                return JsonConvert.DeserializeObject<League[]>(await response.Content.ReadAsStringAsync());
            else
                return new League[] { };
        }

        public async Task<League[]> GetLeaguesByNameAsync(RiotRegion region, string name) =>
            await GetLeaguesBySummonerAsync(region, await GetSummonerByNameAsync(region, name));

        public async Task<string> GetBestSoloRank(RiotRegion region, string name)
        {
            var leagues = await GetLeaguesByNameAsync(region, name);
            if (leagues.Any())
                if (leagues.Where(x => x.QueueType.ToLower() == "ranked_solo_5x5").Any())
                    return leagues.Where(x => x.QueueType.ToLower() == "ranked_solo_5x5").FirstOrDefault().Tier;
            return string.Empty;
        }

        public RiotRegion GetRiotRegion(string guildName)
        {
            if (guildName.Contains("EUW")) return RiotRegion.euw1;
            else return RiotRegion.na1;
        }
    }
}
