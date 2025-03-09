// Controllers/HomeController.cs
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using YoutubeAPI.Models;

namespace YouTubeAPIApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly HttpClient _httpClient;
        private const string ApiKey = "AIzaSyAJKtHsgqlvXbld7rvQBOCSMezHXwGlTB4"; // Replace with your actual API key

        public HomeController()
        {
            _httpClient = new HttpClient();
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Search()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Search(string query)
        {
            var searchUrl = $"https://www.googleapis.com/youtube/v3/search?part=snippet&q={query}&key={ApiKey}&maxResults=10";
            var response = await _httpClient.GetAsync(searchUrl);

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var searchResults = JsonConvert.DeserializeObject<dynamic>(json);

                var videos = new List<YouTubeVideo>();

                foreach (var item in searchResults.items)
                {
                    var video = new YouTubeVideo
                    {
                        Title = item.snippet.title,
                        ChannelTitle = item.snippet.channelTitle,
                        Description = item.snippet.description,
                        ThumbnailUrl = item.snippet.thumbnails.medium.url,
                        PublishedAt = item.snippet.publishedAt,
                        VideoId = item.id.videoId // Extract the video ID
                    };
                    videos.Add(video);
                }

                return View("Results", videos);
            }

            return View("Error");
        }
    }
}