using NashorMatch.Discord.Enums;
using NashorMatch.Discord.Models;
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
            return JsonConvert.DeserializeObject<Summoner>(await response.Content.ReadAsStringAsync());
        }

        async Task<League[]> GetLeaguesBySummonerAsync(RiotRegion region, Summoner summoner)
        {
            var response = await httpService.HttpClient.GetAsync(protocol + region + address + leaguesBySummoner + summoner.Id + api + key);
            return JsonConvert.DeserializeObject<League[]>(await response.Content.ReadAsStringAsync());
        }

        public async Task<League[]> GetLeaguesByNameAsync(RiotRegion region, string name) =>
            await GetLeaguesBySummonerAsync(region, await GetSummonerByNameAsync(region, name));

        public string GetBestSoloRank(League[] leagues) =>
            leagues.Any() ? leagues.Where(x => x.QueueType.ToLower() == "ranked_solo_5x5").FirstOrDefault().Tier ?? string.Empty : string.Empty;

        public RiotRegion Region(string guildName)
        {
            if (guildName.Contains("EUW")) return RiotRegion.euw1;
            else return RiotRegion.na1;
        }
    }
}
