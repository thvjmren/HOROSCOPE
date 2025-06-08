using HoroScope.DAL;
using Microsoft.AspNetCore.Mvc;

namespace HoroScope.Areas.Admin.Controllers
{
    public class ServiceController : Controller
    {
        private readonly AppDbContext _context;

        public ServiceController(AppDbContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            return View();
        }
    }
}
