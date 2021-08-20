using System.Net.Http;
using System.Threading.Tasks;

namespace RottenTomatoes
{
    public static class WebUtils
    {
        public static HttpClient http;

        // Download a string
        public static async Task<string> DownloadString(string url)
        {
            using (var response = await http.GetAsync(url))
                return await response.Content.ReadAsStringAsync();
        }
    }
}