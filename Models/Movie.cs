using System.Text.Json.Serialization;

namespace MovixApp.Models
{
    public class Movie
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;

        [JsonPropertyName("overview")]
        public string Overview { get; set; } = string.Empty;

        [JsonPropertyName("poster_path")]
        public string? PosterPath { get; set; } 

        [JsonPropertyName("vote_average")]
        public double VoteAverage { get; set; }

        
        public string FullPosterPath
        {
            get
            {
                return string.IsNullOrEmpty(PosterPath)
                    ? "/images/no-poster.jpg"
                    : $"https://image.tmdb.org/t/p/w500{PosterPath}";
            }
        }
    }
}
