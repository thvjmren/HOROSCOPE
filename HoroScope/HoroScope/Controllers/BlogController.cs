using Microsoft.AspNetCore.Mvc;

namespace HoroScope.Controllers
{
    public class BlogController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
