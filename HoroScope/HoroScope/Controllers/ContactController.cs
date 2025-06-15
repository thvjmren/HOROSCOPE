using HoroScope.DAL;
using HoroScope.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HoroScope.Controllers
{
    public class ContactController : Controller
    {
        private readonly AppDbContext _context;

        public ContactController(AppDbContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index()
        {
            var settings = await _context.Settings.ToListAsync();
            //var contact = await _context.Contact.FirstOrDefaultAsync();

            var vm = new ContactVM
            {
                Phone = settings.FirstOrDefault(s => s.Key == "Phone")?.Value,
                Email = settings.FirstOrDefault(s => s.Key == "Email")?.Value,
                Address = settings.FirstOrDefault(s => s.Key == "Address")?.Value,
            };

            return View(vm);
        }
    }
}
