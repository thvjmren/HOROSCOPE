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
    public class BlogCategoryController : Controller
    {
        private readonly AppDbContext _context;

        public BlogCategoryController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {

            List<GetBlogCategoryVM> blogCategoryVMs = await _context.BlogCategories
                .Where(bc => !bc.IsDeleted)
                .Select(sc => new GetBlogCategoryVM
                {
                    Id = sc.Id,
                    Name = sc.Name,
                    CreatedAt = sc.CreatedAt,
                    IsDeleted = sc.IsDeleted,
                }).ToListAsync();

            return View(blogCategoryVMs);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateBlogCategoryVM category)
        {
            if (!ModelState.IsValid)
            {
                return View(category);
            }

            bool result = await _context.BlogCategories
            .Where(cs => !cs.IsDeleted)
            .AnyAsync(cs => cs.Name == category.Name);

            if (result)
            {
                ModelState.AddModelError(nameof(CreateBlogCategoryVM.Name), $"This Category: {category.Name} already exists");
                return View(category);
            }

            BlogCategory BlogCategory = new()
            {
                Name = category.Name,
                CreatedAt = DateTime.Now
            };

            await _context.BlogCategories.AddAsync(BlogCategory);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id is null) return BadRequest();

            BlogCategory? category = await _context.BlogCategories
                .FirstOrDefaultAsync(bc => bc.Id == id && bc.IsDeleted == false);

            if (category is null) return NotFound();
            category.IsDeleted = true;
            _context.BlogCategories.Update(category);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> HardDelete(int? id)
        {
            if (id is null) return BadRequest();

            BlogCategory? category = await _context.BlogCategories
                .FirstOrDefaultAsync(sc => sc.Id == id);

            if (category is null) return NotFound();

            _context.BlogCategories.Remove(category);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Update(int? id)
        {
            if (id is null || id <= 0) return BadRequest();

            BlogCategory? category = await _context.BlogCategories.FirstOrDefaultAsync(sc => sc.Id == id);

            if (category is null) return NotFound();

            UpdateBlogCategoryVM categoryVM = new()
            {
                Name = category.Name,
            };

            return View(categoryVM);
        }

        [HttpPost]
        public async Task<IActionResult> Update(int? id, UpdateBlogCategoryVM? categoryVM)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            bool result = await _context.BlogCategories.AnyAsync(c => c.Name == categoryVM.Name && c.Id != id);
            if (result)
            {
                ModelState.AddModelError(nameof(UpdateBlogCategoryVM.Name), $"This Category: This Category: {categoryVM.Name} is already exists");
                return View(categoryVM);
            }

            BlogCategory? existed = await _context.BlogCategories.FirstOrDefaultAsync(c => c.Id == id);

            if (existed.Name == categoryVM.Name) return RedirectToAction(nameof(Index));

            existed.Name = categoryVM.Name;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Restore(int? id)
        {
            if (id is null) return BadRequest();

            BlogCategory? category = await _context.BlogCategories
                .FirstOrDefaultAsync(sc => sc.Id == id && sc.IsDeleted == true);

            if (category is null) return NotFound();

            category.IsDeleted = false;
            _context.BlogCategories.Update(category);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(DeletedCategories));
        }
        public async Task<IActionResult> DeletedCategories()
        {
            var deletedCategories = await _context.BlogCategories
                .Where(sc => sc.IsDeleted == true)
                .ToListAsync();

            return View(deletedCategories);
        }
    }
}
