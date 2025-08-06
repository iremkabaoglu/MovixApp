using Microsoft.AspNetCore.Mvc;
using MovixApp.Models;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace MovixApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey = "b732f83847ff8e725e58e0aeb8e25c02";
        private readonly string _baseUrl = "https://api.themoviedb.org/3/";

        public HomeController(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient();
        }

        // Popüler Filmler
        public async Task<IActionResult> Index()
        {
            string apiUrl = $"{_baseUrl}movie/popular?api_key={_apiKey}&language=tr-TR&page=1";

            var response = await _httpClient.GetAsync(apiUrl);

            if (!response.IsSuccessStatusCode)
            {
                return Content("API'den veri alýnamadý. Hata kodu: " + response.StatusCode);
            }

            var json = await response.Content.ReadAsStringAsync();

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var movieResponse = JsonSerializer.Deserialize<MovieResponse>(json, options);

            if (movieResponse == null || movieResponse.Results == null)
            {
                return Content("API'den geçerli veri alýnamadý.");
            }

            return View(movieResponse.Results);
        }

        // Film Detay Sayfasý
        public async Task<IActionResult> Details(int id)
        {
            string apiUrl = $"{_baseUrl}movie/{id}?api_key={_apiKey}&language=tr-TR";

            var response = await _httpClient.GetAsync(apiUrl);

            if (!response.IsSuccessStatusCode)
            {
                return Content("Detay verisi alýnamadý. Hata kodu: " + response.StatusCode);
            }

            var json = await response.Content.ReadAsStringAsync();

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var movie = JsonSerializer.Deserialize<Movie>(json, options);

            if (movie == null)
            {
                return Content("Film detaylarý bulunamadý.");
            }

            return View(movie);
        }
    }
}
