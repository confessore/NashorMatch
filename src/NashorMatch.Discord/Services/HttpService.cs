using System.Net.Http;

namespace NashorMatch.Discord.Services
{
    public class HttpService
    {
        public HttpClient HttpClient { get; }

        public HttpService()
        {
            HttpClient = new HttpClient();
        }
    }
}
