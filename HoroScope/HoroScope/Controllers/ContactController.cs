using Microsoft.AspNetCore.Mvc;

namespace HoroScope.Controllers
{
    public class ContactController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
