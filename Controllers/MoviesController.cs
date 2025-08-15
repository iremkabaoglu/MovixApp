using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using MovixApp.Models;
using System.Text.Json;

namespace MovixApp.Controllers
{
    public class MoviesController : Controller
    {
        private readonly IHttpClientFactory _http;
        private readonly IConfiguration _cfg;
        private readonly IMemoryCache _cache;
        private string ApiKey => _cfg["TMDB:ApiKey"]!; // appsettings.json -> "TMDB:ApiKey"

        public MoviesController(IHttpClientFactory http, IConfiguration cfg, IMemoryCache cache)
        {
            _http = http;
            _cfg = cfg;
            _cache = cache;
        }

        // POPÜLER FİLMLER (liste)
        public async Task<IActionResult> Index(int page = 1)
        {
            var url = $"https://api.themoviedb.org/3/movie/popular?api_key={ApiKey}&language=tr-TR&page={page}";
            var json = await _http.CreateClient().GetStringAsync(url);
            var opts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var resp = JsonSerializer.Deserialize<MovieResponse>(json, opts);
            return View(resp?.Results ?? new List<Movie>());
        }

        // DETAY
        public async Task<IActionResult> Details(int id)
        {
            var url = $"https://api.themoviedb.org/3/movie/{id}?api_key={ApiKey}&language=tr-TR";
            var json = await _http.CreateClient().GetStringAsync(url);
            var opts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var movie = JsonSerializer.Deserialize<Movie>(json, opts);
            return View(movie);
        }

        // TÜRE GÖRE KEŞFET
        public async Task<IActionResult> ByGenre(int id, int page = 1)
        {
            var url = $"https://api.themoviedb.org/3/discover/movie?api_key={ApiKey}&language=tr-TR&with_genres={id}&page={page}&sort_by=popularity.desc";
            var json = await _http.CreateClient().GetStringAsync(url);
            var opts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var resp = JsonSerializer.Deserialize<MovieResponse>(json, opts);
            return View("Search", resp?.Results ?? new List<Movie>());
        }

        // ARAMA SONUÇLARI
        [HttpGet]
        public async Task<IActionResult> Search(string q, int page = 1)
        {
            if (string.IsNullOrWhiteSpace(q))
                return View(Enumerable.Empty<Movie>());

            var url = $"https://api.themoviedb.org/3/search/movie?api_key={ApiKey}&language=tr-TR&query={Uri.EscapeDataString(q)}&page={page}&include_adult=false";
            var json = await _http.CreateClient().GetStringAsync(url);
            var opts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var resp = JsonSerializer.Deserialize<MovieResponse>(json, opts);
            return View(resp?.Results ?? new List<Movie>());
        }

        // CANLI ÖNERİ (Navbar için küçük JSON API)
        [HttpGet("/api/search/suggest")]
        public async Task<IActionResult> Suggest(string q)
        {
            if (string.IsNullOrWhiteSpace(q) || q.Length < 2)
                return Json(Array.Empty<object>());

            var key = $"sg:{q.ToLower()}";
            if (_cache.TryGetValue(key, out object cached)) return Json(cached);

            var url = $"https://api.themoviedb.org/3/search/movie?api_key={ApiKey}&language=tr-TR&query={Uri.EscapeDataString(q)}&page=1&include_adult=false";
            var json = await _http.CreateClient().GetStringAsync(url);
            var opts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var resp = JsonSerializer.Deserialize<MovieResponse>(json, opts);

            var result = (resp?.Results ?? new())
                .Take(8)
                .Select(m => new {
                    id = m.Id,
                    title = m.Title,
                    poster = string.IsNullOrEmpty(m.PosterPath) ? null : $"https://image.tmdb.org/t/p/w92{m.PosterPath}"
                })
                .ToArray();

            _cache.Set(key, result, TimeSpan.FromMinutes(10));
            return Json(result);
        }
    }
}
