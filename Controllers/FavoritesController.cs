using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MovixApp.Data;
using MovixApp.Models;
using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;

namespace MovixApp.Controllers
{
    [Authorize]
    public class FavoritesController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IHttpClientFactory _http;
        private readonly IConfiguration _cfg;
        private readonly IMemoryCache _cache;

        private string ApiKey => _cfg["TMDB:ApiKey"] ?? string.Empty;
        private static readonly JsonSerializerOptions _jsonOpts =
            new() { PropertyNameCaseInsensitive = true };

        public FavoritesController(
            AppDbContext context,
            UserManager<IdentityUser> userManager,
            IHttpClientFactory http,
            IConfiguration cfg,
            IMemoryCache cache)
        {
            _context = context;
            _userManager = userManager;
            _http = http;
            _cfg = cfg;
            _cache = cache;
        }

        // Küçük JSON GET helper
        private async Task<T?> GetAsync<T>(string url)
        {
            var client = _http.CreateClient();
            using var res = await client.GetAsync(url);
            if (!res.IsSuccessStatusCode) return default;
            var json = await res.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<T>(json, _jsonOpts);
        }

        // Favoriler listesi 
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User)!;

            var ids = await _context.Favorites
                .Where(f => f.UserId == userId)
                .OrderByDescending(f => f.CreatedAt)
                .Select(f => f.MovieId)
                .ToListAsync();

            var tasks = ids.Select(async id =>
            {
                var cacheKey = $"tmdb:movie:{id}:tr";
                if (!_cache.TryGetValue(cacheKey, out Movie? mv))
                {
                    mv = await GetAsync<Movie>(
                        $"https://api.themoviedb.org/3/movie/{id}?api_key={ApiKey}&language=tr-TR");
                    if (mv != null) _cache.Set(cacheKey, mv, TimeSpan.FromHours(12));
                }
                return mv;
            });

            var movies = (await Task.WhenAll(tasks)).Where(m => m != null)!.ToList();
            return View(movies); 
        }

        // Favori ekle
        [HttpPost]
        public async Task<IActionResult> Add(int movieId)
        {
            var userId = _userManager.GetUserId(User)!;

            if (!await _context.Favorites.AnyAsync(f => f.UserId == userId && f.MovieId == movieId))
            {
                _context.Favorites.Add(new Favorite { UserId = userId, MovieId = movieId });
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        // Favoriden kaldır 
        [HttpPost]
        public async Task<IActionResult> RemoveByMovie(int movieId)
        {
            var userId = _userManager.GetUserId(User)!;
            var fav = await _context.Favorites
                .FirstOrDefaultAsync(f => f.UserId == userId && f.MovieId == movieId);
            if (fav != null)
            {
                _context.Favorites.Remove(fav);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
