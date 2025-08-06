using Microsoft.AspNetCore.Mvc;
using MovixApp.Models;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace MovixApp.Controllers
{
    public class MoviesController : Controller
    {
        private readonly HttpClient _httpClient;

        public MoviesController(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<IActionResult> Index()
        {
            string apiKey = "b732f83847ff8e725e58e0aeb8e25c02";
            string url = $"https://api.themoviedb.org/3/movie/popular?api_key={apiKey}&language=tr&page=1";

            var response = await _httpClient.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();

                var movieData = JsonSerializer.Deserialize<MovieResponse>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                return View(movieData.Results);
            }

            return View(new List<Movie>());
        }
    }
}
