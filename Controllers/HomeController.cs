using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using YoutubeAPI.Models;
using System.Collections.Generic;
using System;

namespace YouTubeAPIApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly HttpClient _httpClient;
        private const string ApiKey = "AIzaSyBBVTxvnQpeDDYq5McZ75vHYm81NWxZ-D0"; // Move this to appsettings.json

        public HomeController()
        {
            _httpClient = new HttpClient();
        }

        public async Task<IActionResult> Index()
        {
            var (videos, _, _) = await FetchVideos("", "", "", "", true);
            return View(videos);
        }

        [HttpPost]
        public async Task<IActionResult> Search(string query, string durationFilter, string dateFilter)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return RedirectToAction("Index");
            }

            return await FetchAndDisplayVideos(query, durationFilter, dateFilter, "", 1);
        }

        [HttpGet]
        public async Task<IActionResult> Paginate(string query, string durationFilter, string dateFilter, string pageToken, int currentPage)
        {
            if (currentPage > 10)
            {
                return RedirectToAction("Index");
            }

            return await FetchAndDisplayVideos(query, durationFilter, dateFilter, pageToken, currentPage);
        }

        private async Task<IActionResult> FetchAndDisplayVideos(string query, string durationFilter, string dateFilter, string pageToken, int currentPage)
        {
            var (videos, nextPageToken, prevPageToken) = await FetchVideos(query, durationFilter, dateFilter, pageToken, false);

            ViewBag.CurrentPage = currentPage;
            ViewBag.NextPageToken = nextPageToken;
            ViewBag.PrevPageToken = prevPageToken;
            ViewBag.Query = query;
            ViewBag.DurationFilter = durationFilter;
            ViewBag.DateFilter = dateFilter;

            return View("Results", videos);
        }

        private async Task<(List<YouTubeVideo>, string nextPageToken, string prevPageToken)> FetchVideos(string query, string durationFilter, string dateFilter, string pageToken, bool isTrending)
        {
            try
            {
                string searchUrl = isTrending
                    ? $"https://www.googleapis.com/youtube/v3/videos?part=snippet&chart=mostPopular&regionCode=US&maxResults=12&key={ApiKey}"
                    : $"https://www.googleapis.com/youtube/v3/search?part=snippet&q={query}&type=video&key={ApiKey}&maxResults=12";

                if (!string.IsNullOrEmpty(pageToken))
                {
                    searchUrl += $"&pageToken={pageToken}";
                }

                if (!string.IsNullOrEmpty(durationFilter))
                {
                    searchUrl += $"&videoDuration={durationFilter}";
                }

                if (!string.IsNullOrEmpty(dateFilter))
                {
                    searchUrl += $"&publishedAfter={GetPublishedAfterDate(dateFilter)}";
                }

                var response = await _httpClient.GetAsync(searchUrl);
                if (!response.IsSuccessStatusCode)
                {
                    return (new List<YouTubeVideo>(), "", "");
                }

                var json = await response.Content.ReadAsStringAsync();
                var searchResults = JsonConvert.DeserializeObject<dynamic>(json);
                if (searchResults == null || searchResults.items == null)
                {
                    return (new List<YouTubeVideo>(), "", "");
                }

                var videos = new List<YouTubeVideo>();
                string nextPage = searchResults.nextPageToken?.ToString();
                string prevPage = searchResults.prevPageToken?.ToString();

                foreach (var item in searchResults.items)
                {
                    var videoId = isTrending ? item.id?.ToString() : item.id.videoId?.ToString();
                    if (string.IsNullOrEmpty(videoId) || item?.snippet == null)
                        continue;

                    DateTime publishedAt;
                    var video = new YouTubeVideo
                    {
                        Title = item.snippet.title?.ToString(),
                        ChannelTitle = item.snippet.channelTitle?.ToString(),
                        Description = item.snippet.description?.ToString(),
                        ThumbnailUrl = item.snippet.thumbnails?.medium?.url?.ToString(),
                        VideoId = videoId,
                        PublishedAt = DateTime.TryParse(item.snippet.publishedAt?.ToString(), out publishedAt) ? publishedAt : DateTime.MinValue
                    };

                    videos.Add(video);
                }

                return (videos, nextPage, prevPage);
            }
            catch (Exception)
            {
                return (new List<YouTubeVideo>(), "", "");
            }
        }

        private string GetPublishedAfterDate(string dateFilter)
        {
            if (string.IsNullOrEmpty(dateFilter)) return "";

            DateTime now = DateTime.UtcNow;
            switch (dateFilter)
            {
                case "hour": return now.AddHours(-1).ToString("yyyy-MM-ddTHH:mm:ssZ");
                case "today": return now.Date.ToString("yyyy-MM-ddTHH:mm:ssZ");
                case "week": return now.AddDays(-7).ToString("yyyy-MM-ddTHH:mm:ssZ");
                case "month": return now.AddMonths(-1).ToString("yyyy-MM-ddTHH:mm:ssZ");
                case "year": return now.AddYears(-1).ToString("yyyy-MM-ddTHH:mm:ssZ");
                default: return "";
            }
        }
    }
}
