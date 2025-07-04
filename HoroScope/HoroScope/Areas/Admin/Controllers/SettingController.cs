using Microsoft.AspNetCore.Mvc;

namespace HoroScope.Areas.Admin.Controllers
{
    public class SettingController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
