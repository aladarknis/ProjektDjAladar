using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace ProjektDjAladar
{
    public static class ApiConverter
    {
        public static async Task<string> ApiConvert(string url)
        {
            var client = new HttpClient();
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri("https://youtube-to-mp4.p.rapidapi.com/url=&title?url=https%3A%2F%2F"+url),
                Headers =
                        {
                            { "x-rapidapi-host", "youtube-to-mp4.p.rapidapi.com" },
                            { "x-rapidapi-key", "b7e6fb5313msh2b41d6afdd43dcfp14eacejsnff4c824715ba" },
                        },
            };
            using var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();
           

            return await response.Content.ReadAsStringAsync();
        }
    }
}


