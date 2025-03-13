// Models/YouTubeVideo.cs
namespace YoutubeAPI.Models
{
    public class YouTubeVideo
    {
        public string Title { get; set; }
        public string ChannelTitle { get; set; }
        public string Description { get; set; }
        public string ThumbnailUrl { get; set; }
        public DateTime PublishedAt { get; set; }
        public string VideoId { get; set; } // Add this property for the YouTube video ID
        public string Duration { get; set; } // Add this for duration filtering
    }
}