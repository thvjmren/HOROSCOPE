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

        public async Task<IActionResult> Index(int? categoryId, string? sort, int page = 1)
        {
            int count = await _context.Products.CountAsync();
            int pageSize = 3;
            double total = Math.Ceiling((double)count / pageSize);

            if (page > total) return BadRequest();

            var query = _context.Products.Where(p => !p.IsDeleted);

            if (categoryId != null)
            {
                query = query.Where(p => p.ProductCategoryId == categoryId);
            }

            query = query.Include(p => p.ProductImages);

            switch (sort)
            {
                case "popularity":
                    query = query.OrderByDescending(p => p.SalesCount)
                                 .ThenByDescending(p => p.ViewsCount)
                                 .ThenByDescending(p => p.Rating);
                    break;
                case "priceHighToLow":
                    query = query.OrderByDescending(p => p.Price);
                    break;
                case "priceLowToHigh":
                    query = query.OrderBy(p => p.Price);
                    break;
                case "newest":
                    query = query.OrderByDescending(p => p.Id);
                    break;
                default:
                    query = query.OrderByDescending(p => p.Id);
                    break;
            }

            int totalCount = await query.CountAsync();
            var pagedProducts = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

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
                    .Where(pc => !pc.IsDeleted)
                    .ToListAsync(),

                Products = new PaginatedVM<Product>
                {
                    Items = pagedProducts,
                    CurrentPage = page,
                    TotalPage = Math.Ceiling((double)totalCount / pageSize)
                },

                NewProducts = newProducts,
                ProductCount = totalCount,
                TopCategories = topCategories,
                SelectedSort = sort
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
