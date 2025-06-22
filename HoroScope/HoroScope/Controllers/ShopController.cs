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
                .Include(p => p.ProductReviews)
                .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);

            if (product == null) return NotFound();

            product.ViewsCount += 1;
            await _context.SaveChangesAsync();

            product.ProductImages = product.ProductImages?
                .Where(pi => pi.IsPrimary == true).ToList();

            var popularProducts = await _context.Products
           .Where(p => !p.IsDeleted)
           .Include(p => p.ProductImages.Where(pi => pi.IsPrimary == true))
           .OrderByDescending(p => p.SalesCount)
           .ThenByDescending(p => p.ViewsCount)
           .ThenByDescending(p => p.Rating)
           .Take(3)
           .ToListAsync();

            var vm = new ProductPageVM
            {
                ShopDetailsVM = new ShopDetailsVM
                {
                    Product = product,
                    Reviews = product.ProductReviews,
                    PopularProducts = popularProducts
                },
                ProductReviewVM = new ProductReviewVM
                {
                    ProductId = product.Id
                }
            };

            return View(vm);
        }




        public async Task<IActionResult> Review()
        {
            //var product = await _context.Products
            //    .Include(p => p.ProductReviews)
            //    .FirstOrDefaultAsync(p => p.Id == productId && !p.IsDeleted);

            //if (product == null)
            //    return NotFound();

            //var vm = new ShopDetailsVM
            //{
            //    Product = product,
            //    Reviews = product.ProductReviews
            //};

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Review(ProductPageVM vm)
        {
            if (!ModelState.IsValid)
            {
                var product = await _context.Products
                    .Include(p => p.ProductImages)
                    .Include(p => p.ProductFeatureValues)
                        .ThenInclude(pfv => pfv.FeatureValue)
                        .ThenInclude(fv => fv.Feature)
                    .Include(p => p.ProductReviews)
                    .FirstOrDefaultAsync(p => p.Id == vm.ProductReviewVM.ProductId && !p.IsDeleted);

                if (product == null)
                {
                    return NotFound();
                }

                vm.ShopDetailsVM = new ShopDetailsVM
                {
                    Product = product,
                    Reviews = product.ProductReviews
                };

                vm.ProductReviewVM ??= new ProductReviewVM
                {
                    ProductId = product.Id
                };

                return View("Details", vm);
            }

            var review = new ProductReview
            {
                ProductId = vm.ProductReviewVM.ProductId,
                ReviewerName = User.Identity.Name,
                Comment = vm.ProductReviewVM.Comment,
                Rating = vm.ProductReviewVM.Rating,
                CreatedAt = DateTime.UtcNow
            };

            await _context.ProductReviews.AddAsync(review);
            await _context.SaveChangesAsync();

            var productToUpdate = await _context.Products
            .Include(p => p.ProductReviews)
            .FirstOrDefaultAsync(p => p.Id == vm.ProductReviewVM.ProductId);

            if (productToUpdate != null)
            {
                productToUpdate.ReviewCount = productToUpdate.ProductReviews.Count;
                productToUpdate.Rating = productToUpdate.ProductReviews.Any()
                    ? productToUpdate.ProductReviews.Average(r => r.Rating)
                    : 0;

                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Details", new { id = vm.ProductReviewVM.ProductId });
        }


    }
}
