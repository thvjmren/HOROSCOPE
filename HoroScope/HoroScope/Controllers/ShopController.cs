using HoroScope.DAL;
using HoroScope.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HoroScope.Controllers
{
    public class ShopController : Controller
    {
        private readonly AppDbContext _context;

        public ShopController(AppDbContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index(int? categoryId)
        {
            var products = categoryId == null
            ? await _context.Products.Where(p => !p.IsDeleted).Include(p => p.ProductImages.Where(pi => pi.IsPrimary != null)).ToListAsync()
            : await _context.Products.Where(p => p.ProductCategoryId == categoryId && !p.IsDeleted).Include(p => p.ProductImages.Where(pi => pi.IsPrimary != null)).ToListAsync();

            var newProducts = await _context.Products
                .Include(p => p.ProductImages.Where(pi => pi.IsPrimary != null))
                .Where(p => !p.IsDeleted)
                .OrderByDescending(p => p.Id)
                .Take(3)
                .ToListAsync();

            var topCategories = await _context.ProductCategories
                .Where(p => !p.IsDeleted)
                .OrderByDescending(pc => pc.Products.Count(p => !p.IsDeleted))
                .Take(5)
                .ToListAsync();

            ShopVM vm = new()
            {
                ProductCategories = await _context.ProductCategories
                .Where(pc => pc.IsDeleted == false)
                .ToListAsync(),

                Products = products,

                NewProducts = newProducts,

                ProductCount = products.Count,

                TopCategories = topCategories,
            };
            return View(vm);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id is null || id <= 0) return BadRequest();

            var product = await _context.Products.Include(p => p.ProductImages.Where(pi => pi.IsPrimary != null)).FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);

            if (product is null) return NotFound();

            ShopDetailsVM vm = new ShopDetailsVM
            {
                Product = product
            };

            return View(vm);
        }

    }
}
