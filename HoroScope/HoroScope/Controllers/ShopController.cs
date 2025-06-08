using Microsoft.AspNetCore.Mvc;

namespace HoroScope.Controllers
{
    public class ShopController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
