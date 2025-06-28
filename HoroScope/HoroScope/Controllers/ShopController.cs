using HoroScope.DAL;
using HoroScope.Models;
using HoroScope.Utilities.Enums;
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
        public async Task<IActionResult> Index(string? search, int? categoryId, int key = 1, int page = 1)
        {
            int pageSize = 3;

            IQueryable<Product> query = _context.Products
                .Where(p => !p.IsDeleted);

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(p => p.Name.ToLower().Contains(search.ToLower()));
            }

            if (categoryId != null && categoryId > 0)
            {
                query = query.Where(p => p.ProductCategoryId == categoryId);
            }

            query = query.Include(p => p.ProductImages);

            switch (key)
            {
                case (int)SortType.Name:
                    query = query.OrderByDescending(p => p.SalesCount)
                                 .ThenByDescending(p => p.ViewsCount)
                                 .ThenByDescending(p => p.Rating);
                    break;
                case (int)SortType.Price:
                    query = query.OrderByDescending(p => p.Price);
                    break;
                case (int)SortType.Date:
                    query = query.OrderBy(p => p.CreatedAt);
                    break;
            }

            int totalCount = await query.CountAsync();
            double totalPage = Math.Ceiling((double)totalCount / pageSize);

            if (page > totalPage && totalPage != 0)
                return BadRequest();

            var pagedProducts = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var newProducts = await _context.Products
                .Where(p => !p.IsDeleted)
                .Include(p => p.ProductImages.Where(pi => pi.IsPrimary == true))
                .OrderByDescending(p => p.Id)
                .Take(3)
                .ToListAsync();

            var topCategories = await _context.ProductCategories
                .Where(pc => !pc.IsDeleted)
                .OrderByDescending(pc => pc.Products.Count(p => !p.IsDeleted))
                .Take(5)
                .ToListAsync();

            var productCategories = await _context.ProductCategories
                .Where(pc => !pc.IsDeleted)
                .ToListAsync();

            ShopVM vm = new()
            {
                ProductCategories = productCategories,
                Products = new PaginatedVM<Product>
                {
                    Items = pagedProducts,
                    CurrentPage = page,
                    TotalPage = totalPage
                },
                NewProducts = newProducts,
                ProductCount = totalCount,
                TopCategories = topCategories,
                Search = search,
                SelectedCategoryId = categoryId,
                Key = key
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
                    Reviews = product.ProductReviews.ToList(),
                    PopularProducts = popularProducts
                },
                ProductReviewVM = new ProductReviewVM
                {
                    ProductId = product.Id
                }
            };

            return View(vm);
        }

        public IActionResult Review(int id)
        {
            return RedirectToAction("Details", new { id });
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Review(ProductReviewVM model)
        {
            if (!ModelState.IsValid)
            {
                var product = await _context.Products
                    .Include(p => p.ProductImages)
                    .Include(p => p.ProductFeatureValues)
                        .ThenInclude(pfv => pfv.FeatureValue)
                        .ThenInclude(fv => fv.Feature)
                    .Include(p => p.ProductReviews)
                    .FirstOrDefaultAsync(p => p.Id == model.ProductId && !p.IsDeleted);

                if (product == null)
                {
                    return NotFound();
                }

                var popularProducts = await _context.Products
                    .Where(p => !p.IsDeleted && p.Id != product.Id)
                    .OrderByDescending(p => p.Rating)
                    .Take(4)
                    .ToListAsync();

                var vm = new ProductPageVM
                {
                    ShopDetailsVM = new ShopDetailsVM
                    {
                        Product = product,
                        Reviews = product.ProductReviews,
                        PopularProducts = popularProducts
                    },
                    ProductReviewVM = model
                };

                ModelState.Clear();

                return View("Details", vm);
            }

            var review = new ProductReview
            {
                ProductId = model.ProductId,
                ReviewerName = User.Identity.Name,
                Comment = model.Comment,
                Rating = model.Rating,
                CreatedAt = DateTime.UtcNow
            };

            await _context.ProductReviews.AddAsync(review);
            await _context.SaveChangesAsync();

            var productToUpdate = await _context.Products
                .Include(p => p.ProductReviews)
                .FirstOrDefaultAsync(p => p.Id == model.ProductId);

            if (productToUpdate != null)
            {
                productToUpdate.ReviewCount = productToUpdate.ProductReviews.Count;
                productToUpdate.Rating = productToUpdate.ProductReviews.Any()
                    ? productToUpdate.ProductReviews.Average(r => r.Rating)
                    : 0;

                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Details", new { id = model.ProductId });
        }



        [HttpGet]
        public async Task<IActionResult> CheckPinCode(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
                return Json(new { isDeliverable = false, message = "Zip code cannot be empty." });

            var normalizedCode = code.Trim().ToUpper();

            var isValidFormat = System.Text.RegularExpressions.Regex.IsMatch(
                normalizedCode,
                @"^(AZ\d{4}|\d{5,6})$"
            );

            if (!isValidFormat)
            {
                return Json(new
                {
                    isDeliverable = false,
                    isFormatError = true,
                    message = "❌ Please enter a valid zip code format (for ex. AZ1000 / 90210)"
                });
            }

            bool isDeliverable = await _context.DeliverableAddress
                .AnyAsync(d => d.PinCode.ToUpper() == normalizedCode && d.IsActive);

            return Json(new
            {
                isDeliverable,
                isFormatError = false,
                message = isDeliverable
                    ? "✅ Delivery available to this area."
                    : "❌ Sorry, we don't deliver to this area."
            });
        }




    }
}
