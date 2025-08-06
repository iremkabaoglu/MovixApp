using System.Text.Json.Serialization;

namespace MovixApp.Models
{
    public class Movie
    {
        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("overview")]
        public string Overview { get; set; }

        [JsonPropertyName("poster_path")]
        public string PosterPath { get; set; }

        [JsonPropertyName("vote_average")]
        public double VoteAverage { get; set; }
    }

    public class MovieResponse
    {
        [JsonPropertyName("results")]
        public List<Movie> Results { get; set; }
    }
}
