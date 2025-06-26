using HoroScope.DAL;
using HoroScope.Models;
using HoroScope.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HoroScope.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
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
                IsFree = s.IsFree,
                Price = s.Price
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
                ModelState.AddModelError(nameof(CreateServiceVM), $"This Service:{serviceVM.Name} is already exist");
                return View(serviceVM);
            }

            bool serviceResult = serviceVM.ServiceCategories.Any(c => c.Id != serviceVM.CategoryId);
            if (!serviceResult)
            {
                ModelState.AddModelError(nameof(CreateServiceVM.CategoryId), $"This Service Category does not exist");
                return View(serviceVM);
            }

            bool iconResult = await _context.Services.AnyAsync(s => s.Icon == serviceVM.Icon);
            if (iconResult)
            {
                ModelState.AddModelError(nameof(CreateServiceVM.CategoryId), "This icon is already used");
                return View(serviceVM);
            }

            var category = await _context.ServiceCategories.FindAsync(serviceVM.CategoryId);
            if (category == null)
            {
                ModelState.AddModelError(nameof(CreateServiceVM.CategoryId), $"This Service Category does not exist");
                return View(serviceVM);
            }

            Service service = new()
            {
                Name = serviceVM.Name,
                ServiceCategoryId = serviceVM.CategoryId,
                Description = serviceVM.Description,
                CreatedAt = DateTime.Now,
                Icon = serviceVM.Icon,
                IsFree = category.Name.ToLower().Contains("free"),
                Price = category.Name.ToLower().Contains("free") ? 0 : serviceVM.Price
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
                ServiceCategories = await _context.ServiceCategories.ToListAsync(),
                IsFree = service.IsFree,
                Price = service.Price,
            };

            return View(serviceVM);
        }

        [HttpPost]
        public async Task<IActionResult> Update(int? id, UpdateServiceVM serviceVM)
        {
            serviceVM.ServiceCategories = await _context.ServiceCategories.ToListAsync();

            if (!ModelState.IsValid) return View(serviceVM);

            if (id == null) return BadRequest();

            var service = await _context.Services.FindAsync(id);

            if (service == null) return NotFound();

            bool result = await _context.Services.AnyAsync(s => s.Name == serviceVM.Name && s.Id != id);
            if (result)
            {
                ModelState.AddModelError(nameof(serviceVM.Name), $"This service name '{serviceVM.Name}' already exists.");
                return View(serviceVM);
            }

            bool serviceResult = await _context.ServiceCategories.AnyAsync(c => c.Id == serviceVM.CategoryId);
            if (!serviceResult)
            {
                ModelState.AddModelError(nameof(serviceVM.CategoryId), "This service category does not exist.");
                return View(serviceVM);
            }

            bool iconResult = await _context.Services.AnyAsync(s => s.Icon == serviceVM.Icon && s.Id != id);
            if (iconResult)
            {
                ModelState.AddModelError(nameof(serviceVM.Icon), "This icon is already used by another service.");
                return View(serviceVM);
            }


            var category = await _context.ServiceCategories.FindAsync(serviceVM.CategoryId);
            if (category == null)
            {
                ModelState.AddModelError(nameof(serviceVM.CategoryId), "This service category does not exist.");
                return View(serviceVM);
            }

            service.Name = serviceVM.Name;
            service.ServiceCategoryId = serviceVM.CategoryId;
            service.Description = serviceVM.Description;
            service.Icon = serviceVM.Icon;

            service.IsFree = category.Name.ToLower().Contains("free");

            if (service.IsFree)
            {
                service.Price = 0;
            }
            else
            {
                service.Price = serviceVM.Price;
            }


            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

    }
}
