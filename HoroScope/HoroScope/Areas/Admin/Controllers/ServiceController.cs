using HoroScope.DAL;
using HoroScope.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HoroScope.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ServiceController : Controller
    {
        private readonly AppDbContext _context;

        public ServiceController(AppDbContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index()
        {
            List<GetServiceVM> serviceVMs = await _context.Services.Select(s => new GetServiceVM
            {
                Id = s.Id,
                Name = s.Name,
                Icon = s.Icon,
                Description = s.Description,
                CategoryName = s.ServiceCategory.Name,
                CreatedAt = s.CreatedAt,
                IsDeleted = s.IsDeleted,
            }).ToListAsync();
            return View(serviceVMs);
        }
    }
}
