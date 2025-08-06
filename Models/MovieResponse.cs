using System.Text.Json.Serialization;
using System.Collections.Generic;

namespace MovixApp.Models
{
    public class MovieResponse
    {
        [JsonPropertyName("results")]
        public List<Movie>? Results { get; set; }  
    }
}
