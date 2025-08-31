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

    
        private string ApiKey => _cfg["TMDB:ApiKey"] ?? string.Empty;

       
        private static readonly JsonSerializerOptions _jsonOpts =
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

        public MoviesController(IHttpClientFactory http, IConfiguration cfg, IMemoryCache cache)
        {
            _http = http;
            _cfg = cfg;
            _cache = cache;
        }

        // -------------------- Helpers --------------------

        // Genel GET helper: 200 değilse default(T)
        private async Task<T?> GetAsync<T>(string url)
        {
            var client = _http.CreateClient();
            using var res = await client.GetAsync(url);
            if (!res.IsSuccessStatusCode) return default;
            var json = await res.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<T>(json, _jsonOpts);
        }

        // Tür listesini TMDB'den al ve 24 saat cache'le
        private async Task<List<Genre>> GetGenresAsync()
        {
            const string cacheKey = "tmdb:genres:tr";

            if (_cache.TryGetValue(cacheKey, out List<Genre>? cached) && cached is not null)
                return cached;

            if (string.IsNullOrWhiteSpace(ApiKey))
                return new List<Genre>();

            var resp = await GetAsync<GenreResponse>(
                $"https://api.themoviedb.org/3/genre/movie/list?api_key={ApiKey}&language=tr-TR");

            var genres = resp?.Genres ?? new List<Genre>();
            _cache.Set(cacheKey, genres, TimeSpan.FromHours(24));
            return genres;
        }

        // -------------------- Actions --------------------

        // ANA SAYFA: Popüler filmler
        public async Task<IActionResult> Index(int page = 1)
        {
            if (string.IsNullOrWhiteSpace(ApiKey))
                return Problem("TMDB API anahtarı bulunamadı. appsettings.json -> \"TMDB:ApiKey\"");

            ViewBag.ShowSidebar = true;                 // <- sabit sidebar
            ViewBag.Genres = await GetGenresAsync();

            var resp = await GetAsync<MovieResponse>(
                $"https://api.themoviedb.org/3/movie/popular?api_key={ApiKey}&language=tr-TR&page={page}");

            var list = resp?.Results ?? new List<Movie>();
            return View(list);
        }

        // DETAY
        public async Task<IActionResult> Details(int id)
        {
            if (string.IsNullOrWhiteSpace(ApiKey))
                return Problem("TMDB API anahtarı bulunamadı.");

            ViewBag.ShowSidebar = false; 
            ViewBag.Genres = await GetGenresAsync();

            var movie = await GetAsync<Movie>(
                $"https://api.themoviedb.org/3/movie/{id}?api_key={ApiKey}&language=tr-TR");

            if (movie is null) return NotFound();
            return View(movie);
        }

        // TÜRE GÖRE KEŞFET (sol menü)
        public async Task<IActionResult> ByGenre(int id, int page = 1)
        {
            if (string.IsNullOrWhiteSpace(ApiKey))
                return Problem("TMDB API anahtarı bulunamadı.");

            ViewBag.ShowSidebar = true;                 
            ViewBag.Genres = await GetGenresAsync();
            ViewBag.SelectedGenreId = id;

            var resp = await GetAsync<MovieResponse>(
                $"https://api.themoviedb.org/3/discover/movie?api_key={ApiKey}&language=tr-TR&with_genres={id}&page={page}&sort_by=popularity.desc");

            var list = resp?.Results ?? new List<Movie>();
           
            return View("Search", list);
        }

        // ARAMA
        [HttpGet]
        public async Task<IActionResult> Search(string? q, int page = 1)
        {
            ViewBag.ShowSidebar = true;                
            ViewBag.Genres = await GetGenresAsync();
            ViewBag.Query = q ?? string.Empty;

            if (string.IsNullOrWhiteSpace(q))
                return View(Enumerable.Empty<Movie>());

            if (string.IsNullOrWhiteSpace(ApiKey))
                return Problem("TMDB API anahtarı bulunamadı.");

            var resp = await GetAsync<MovieResponse>(
                $"https://api.themoviedb.org/3/search/movie?api_key={ApiKey}&language=tr-TR&query={Uri.EscapeDataString(q!)}&page={page}&include_adult=false");

            var list = resp?.Results ?? new List<Movie>();
            return View(list);
        }

        // NAVBAR canlı öneri (JSON)
        [HttpGet("api/search/suggest")]
        public async Task<IActionResult> Suggest(string? q)
        {
            if (string.IsNullOrWhiteSpace(q) || q.Length < 2)
                return Json(Array.Empty<object>());

            var key = $"sg:{q.ToLower()}";
            if (_cache.TryGetValue(key, out object? cached) && cached is not null)
                return Json(cached);

            if (string.IsNullOrWhiteSpace(ApiKey))
                return Json(Array.Empty<object>());

            var resp = await GetAsync<MovieResponse>(
                $"https://api.themoviedb.org/3/search/movie?api_key={ApiKey}&language=tr-TR&query={Uri.EscapeDataString(q)}&page=1&include_adult=false");

            var result = (resp?.Results ?? new List<Movie>())
                .Take(8)
                .Select(m => new
                {
                    id = m.Id,
                    title = m.Title,
                    poster = string.IsNullOrEmpty(m.PosterPath)
                        ? (string?)null
                        : $"https://image.tmdb.org/t/p/w92{m.PosterPath}"
                })
                .ToArray();

            _cache.Set(key, result, TimeSpan.FromMinutes(10));
            return Json(result);
        }
    }

    
    public class GenreResponse { public List<Genre> Genres { get; set; } = new(); }
    public class Genre { public int Id { get; set; } public string Name { get; set; } = string.Empty; }
}
