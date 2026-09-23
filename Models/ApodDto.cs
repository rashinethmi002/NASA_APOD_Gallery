using System;
using System.Text.Json.Serialization;

namespace NASA_APOD_Gallery.Models
{
    public class ApodDto
    {
        [JsonPropertyName("date")]
        public string? Date { get; set; }

        [JsonPropertyName("title")]
        public string? Title { get; set; }

        [JsonPropertyName("explanation")]
        public string? Explanation { get; set; }

        // Map both url and hdurl; prefer hdurl when present.
        [JsonPropertyName("url")]
        public string? Url { get; set; }

        [JsonPropertyName("hdurl")]
        public string? HdUrl { get; set; }

        [JsonPropertyName("media_type")]
        public string? MediaType { get; set; }

        [JsonPropertyName("service_version")]
        public string? ServiceVersion { get; set; }

        // Helper property used internally to pick the best image URL.
        [JsonIgnore]
        public string BestUrl => !string.IsNullOrWhiteSpace(HdUrl) ? HdUrl : Url;

        // Normalized image URL suitable for <img src="...">. Ensures scheme present.
        [JsonIgnore]
        public string? ImageUrl
        {
            get
            {
                var u = BestUrl ?? Url;
                if (string.IsNullOrWhiteSpace(u)) return u;

                if (u.StartsWith("//")) return "https:" + u;
                if (u.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                    u.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                    return u;

                return "https://" + u;
            }
        }

        [JsonIgnore]
        public bool IsImage
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(MediaType) && string.Equals(MediaType?.Trim(), "image", StringComparison.OrdinalIgnoreCase))
                    return true;

                var img = ImageUrl;
                if (string.IsNullOrWhiteSpace(img)) return false;

                // Check common image file extensions as a fallback when media_type is missing or incorrect.
                var lower = img.ToLowerInvariant();
                if (lower.EndsWith(".jpg") || lower.EndsWith(".jpeg") || lower.EndsWith(".png") || lower.EndsWith(".gif") || lower.EndsWith(".webp") || lower.EndsWith(".bmp"))
                    return true;

                return false;
            }
        }

        // Embed URL for video media (YouTube/Vimeo); fallback to BestUrl.
        [JsonIgnore]
        public string EmbedUrl
        {
            get
            {
                var u = BestUrl ?? Url;
                if (string.IsNullOrWhiteSpace(u)) return u;

                // YouTube watch -> embed
                if (u.Contains("youtube.com/watch?v="))
                {
                    return u.Replace("watch?v=", "embed/");
                }

                // youtu.be short link -> embed
                if (u.Contains("youtu.be/"))
                {
                    var id = u.Substring(u.LastIndexOf('/') + 1);
                    return $"https://www.youtube.com/embed/{id}";
                }

                // Vimeo embed pattern: convert to player.vimeo.com/video/{id}
                if (u.Contains("vimeo.com/") && !u.Contains("player.vimeo.com"))
                {
                    var id = u.Substring(u.LastIndexOf('/') + 1);
                    return $"https://player.vimeo.com/video/{id}";
                }

                return u;
            }
        }
    }
}
