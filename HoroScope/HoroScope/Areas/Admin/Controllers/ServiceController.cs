using HoroScope.DAL;
using HoroScope.Models;
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

        public async Task<IActionResult> Create()
        {
            CreateServiceVM serviceVM = new()
            {
                ServiceCategories = await _context.ServiceCategories.ToListAsync()
            };
            return View(serviceVM);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateServiceVM serviceVM)
        {
            serviceVM.ServiceCategories = await _context.ServiceCategories.ToListAsync();

            if (!ModelState.IsValid) return View(serviceVM);

            bool result = await _context.Services.AnyAsync(s => s.Name == serviceVM.Name);
            if (result)
            {
                ModelState.AddModelError(nameof(CreateServiceVM), $"this Service:{serviceVM.Name} is already exist");
                return View(serviceVM);
            }

            bool serviceResult = serviceVM.ServiceCategories.Any(c => c.Id != serviceVM.CategoryId);
            if (!serviceResult)
            {
                ModelState.AddModelError(nameof(CreateServiceVM.CategoryId), $"this Service Category does not exist");
                return View(serviceVM);
            }

            bool iconResult = await _context.Services.AnyAsync(s => s.Icon == serviceVM.Icon);
            if (iconResult)
            {
                ModelState.AddModelError(nameof(CreateServiceVM.CategoryId), "This icon is already used");
                return View(serviceVM);
            }

            Service service = new()
            {
                Name = serviceVM.Name,
                ServiceCategoryId = serviceVM.CategoryId,
                Description = serviceVM.Description,
                CreatedAt = DateTime.Now,
                Icon = serviceVM.Icon,
            };

            await _context.Services.AddAsync(service);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id is null) return BadRequest();

            Service? service = await _context.Services.Where(c => c.IsDeleted == false).FirstOrDefaultAsync(s => s.Id == id);
            if (service is null) return NotFound();

            _context.Services.Remove(service);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Update(int? id)
        {
            if (id is null) return BadRequest();

            Service? service = await _context.Services.Where(c => c.IsDeleted == false).FirstOrDefaultAsync(s => s.Id == id);
            if (service is null) return NotFound();

            UpdateServiceVM serviceVM = new()
            {
                Name = service.Name,
                Icon = service.Icon,
                Description = service.Description,
                ServiceCategories = await _context.ServiceCategories.ToListAsync()
            };

            return View(serviceVM);
        }

        [HttpPost]
        public async Task<IActionResult> Update(int? id, UpdateServiceVM serviceVM)
        {
            serviceVM.ServiceCategories = await _context.ServiceCategories.ToListAsync();

            if (!ModelState.IsValid) return View(serviceVM);

            bool result = await _context.Services.AnyAsync(s => s.Name == serviceVM.Name && s.Id != serviceVM.CategoryId);
            if (result)
            {
                ModelState.AddModelError(nameof(CreateServiceVM), $"this Service:{serviceVM.Name} is already exist");
                return View(serviceVM);
            }

            bool serviceResult = serviceVM.ServiceCategories.Any(c => c.Id != serviceVM.CategoryId);
            if (!serviceResult)
            {
                ModelState.AddModelError(nameof(CreateServiceVM.CategoryId), $"this Service Category does not exist");
                return View(serviceVM);
            }

            bool iconResult = await _context.Services.AnyAsync(s => s.Icon == serviceVM.Icon && s.Id != serviceVM.CategoryId);
            if (iconResult)
            {
                ModelState.AddModelError(nameof(CreateServiceVM.CategoryId), "This icon is already used");
                return View(serviceVM);
            }

            Service service = new()
            {
                Name = serviceVM.Name,
                ServiceCategoryId = serviceVM.CategoryId,
                Description = serviceVM.Description,
                CreatedAt = DateTime.Now,
                Icon = serviceVM.Icon,
            };

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
