using HoroScope.DAL;
using HoroScope.Models;
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
            ? await _context.Products
            .Where(p => !p.IsDeleted)
            .Include(p => p.ProductImages
            .Where(pi => pi.IsPrimary != null)).ToListAsync()
            : await _context.Products
            .Where(p => p.ProductCategoryId == categoryId && !p.IsDeleted)
            .Include(p => p.ProductImages
            .Where(pi => pi.IsPrimary != null)).ToListAsync();

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

            var product = await _context.Products
            .Include(p => p.ProductImages)
            .Include(p => p.ProductFeatureValues)
            .ThenInclude(pfv => pfv.FeatureValue)
            .ThenInclude(fv => fv.Feature)
            .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);

            if (product == null)
                return NotFound();

            product.ProductImages = product.ProductImages?
                .Where(pi => pi.IsPrimary == true)
                .ToList();

            ShopDetailsVM vm = new ShopDetailsVM
            {
                Product = product
            };

            return View(vm);
        }


        public async Task<IActionResult> Review(int productId)
        {
            var product = await _context.Products
                .Include(p => p.ProductReviews)
                .FirstOrDefaultAsync(p => p.Id == productId && !p.IsDeleted);

            if (product == null)
                return NotFound();

            var vm = new ShopDetailsVM
            {
                Product = product,
                Reviews = product.ProductReviews
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Review(ProductReviewVM vm)
        {
            if (!ModelState.IsValid)
            {
                var product = await _context.Products
                    .Include(p => p.ProductReviews)
                    .FirstOrDefaultAsync(p => p.Id == vm.ProductId);

                var detailsVM = new ShopDetailsVM
                {
                    Product = product,
                    Reviews = product.ProductReviews
                };

                return View("Details", detailsVM);
            }

            var review = new ProductReview
            {
                ProductId = vm.ProductId,
                ReviewerName = User.Identity.Name,
                Comment = vm.Comment,
                Rating = vm.Rating,
                CreatedAt = DateTime.UtcNow
            };

            await _context.ProductReviews.AddAsync(review);
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", new { id = vm.ProductId });
        }
    }
}
