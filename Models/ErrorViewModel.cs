namespace YoutubeAPI.Models
{
    public class ErrorViewModel
    {
        public string RequestId { get; set; }

        // Computed property to check if RequestId is not null or empty
        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}