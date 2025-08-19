using Microsoft.AspNetCore.Mvc;

namespace MovixApp.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index() => RedirectToAction("Index", "Movies");
        public IActionResult Details(int id) => RedirectToAction("Details", "Movies", new { id });
    }
}
