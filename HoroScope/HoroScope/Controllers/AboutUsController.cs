using HoroScope.DAL;
using HoroScope.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HoroScope.Controllers
{
    public class AboutUsController : Controller
    {
        private readonly AppDbContext _context;

        public AboutUsController(AppDbContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index(int? categoryId)
        {
            var services = categoryId == null
                ? await _context.Services.Where(s => !s.IsDeleted).ToListAsync()
                : await _context.Services.Where(s => s.ServiceCategoryId == categoryId && !s.IsDeleted).ToListAsync();

            AboutUsVM vm = new()
            {
                Services = services,
                ServiceCategories = await _context.ServiceCategories.Where(s => !s.IsDeleted).ToListAsync(),
                AboutUs = await _context.AboutUs.Where(s => !s.IsDeleted).ToListAsync(),
                Partners = await _context.Partners.Where(s => !s.IsDeleted).ToListAsync(),
                Experts = await _context.Experts.Where(e => !e.IsDeleted).ToListAsync(),
                ContactNumber = _context.Settings.Where(s => s.Key == "Phone")
                                         .Select(s => s.Value)
                                         .FirstOrDefault()
            };

            return View(vm);
        }
    }
}
