using HoroScope.DAL;
using HoroScope.Models;
using HoroScope.Utilities.Enums;
using HoroScope.Utilities.Extensions;
using HoroScope.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HoroScope.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class ProductController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;

        public ProductController(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        public async Task<IActionResult> Index(int page = 1)
        {
            int count = await _context.Products.CountAsync();
            int pageSize = 3;
            int totalPage = (int)Math.Ceiling((double)count / pageSize);

            if (page < 1 || page > totalPage)
                return BadRequest();

            var products = await _context.Products
                .Include(p => p.ProductCategory)
                .Include(p => p.ProductImages)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new GetProductVM
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    Price = p.Price,
                    Discount = p.Discount,
                    CategoryName = p.ProductCategory.Name,
                    MainImage = p.ProductImages.FirstOrDefault(pi => pi.IsPrimary == true).Image,
                    Stock = p.Stock,
                    SalesCount = p.SalesCount,
                    ViewsCount = p.ViewsCount,
                    Rating = p.Rating,
                    ReviewCount = p.ReviewCount,
                    FreeShipping = p.FreeShipping,
                    CodAvailable = p.CodAvailable,
                    ShippingDays = p.ShippingDays,
                }).ToListAsync();

            var paginatedVM = new PaginatedVM<GetProductVM>
            {
                TotalPage = totalPage,
                CurrentPage = page,
                Items = products
            };

            return View(paginatedVM);
        }

        public async Task<IActionResult> Create()
        {
            var categories = await _context.ProductCategories.ToListAsync();
            var zodiacs = await _context.Zodiacs.ToListAsync();

            var features = await _context.Features
                .Include(f => f.FeatureValues)
                .ToListAsync();

            var featureSelections = features.Select(f => new FeatureSelectionVM
            {
                FeatureId = f.Id,
                FeatureName = f.Name,
                Values = f.FeatureValues,
                SelectedValueIds = new List<int>()
            }).ToList();

            CreateProductVM vm = new CreateProductVM
            {
                Categories = categories,
                FeatureSelections = featureSelections,
                Zodiacs = zodiacs,
            };

            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateProductVM productVM)
        {
            productVM.Categories = await _context.ProductCategories.ToListAsync();
            productVM.Zodiacs = await _context.Zodiacs.ToListAsync();

            if (!ModelState.IsValid)
            {
                return View(productVM);
            }

            bool result = productVM.Categories.Any(c => c.Id == productVM.CategoryId);
            if (!result)
            {
                ModelState.AddModelError(nameof(CreateProductVM.CategoryId), "category does not exist");
                return View(productVM);
            }

            if (!productVM.MainPhoto.ValidateType("image/"))
            {
                ModelState.AddModelError(nameof(CreateProductVM.MainPhoto), "file type is incorrect");
                return View(productVM);
            }

            if (!productVM.MainPhoto.ValidateSize(FileSize.KB, 500))
            {
                ModelState.AddModelError(nameof(CreateProductVM.MainPhoto), "file size must be less than 500 KB");
                return View(productVM);
            }

            bool nameResult = await _context.Products.AnyAsync(p => p.Name == productVM.Name);
            if (nameResult)
            {
                ModelState.AddModelError(nameof(productVM.Name), $"same name:{productVM.Name} is already used");
                return View(productVM);
            }

            ProductImages mainImage = new ProductImages
            {
                Image = await productVM.MainPhoto.CreateFileAsync(_env.WebRootPath, "assets", "images", "content", "shop"),
                IsPrimary = true,
                CreatedAt = DateTime.Now
            };

            Product product = new Product
            {
                Name = productVM.Name,
                Price = productVM.Price.Value,
                Description = productVM.Description,
                ProductCategoryId = productVM.CategoryId.Value,
                ProductImages = new List<ProductImages> { mainImage },
                CreatedAt = DateTime.UtcNow,
            };

            await _context.Products.AddAsync(product);
            await _context.SaveChangesAsync();

            if (productVM.SelectedZodiacIds != null && productVM.SelectedZodiacIds.Any())
            {
                foreach (var zodiacId in productVM.SelectedZodiacIds)
                {
                    var productZodiac = new ProductZodiac
                    {
                        ProductId = product.Id,
                        ZodiacId = zodiacId
                    };
                    await _context.AddAsync(productZodiac);
                }
                await _context.SaveChangesAsync();
            }

            if (productVM.FeatureSelections != null)
            {
                foreach (var featureSelection in productVM.FeatureSelections)
                {
                    if (featureSelection.SelectedValueIds != null)
                    {
                        foreach (var featureValueId in featureSelection.SelectedValueIds)
                        {
                            var productFeatureValue = new ProductFeatureValue
                            {
                                ProductId = product.Id,
                                FeatureValueId = featureValueId
                            };
                            await _context.ProductFeatureValues.AddAsync(productFeatureValue);
                        }
                    }
                }
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id is null || id <= 0) return BadRequest();

            Product? product = await _context.Products.Include(p => p.ProductImages).FirstOrDefaultAsync(p => p.Id == id);

            if (product is null) return NotFound();

            foreach (ProductImages productImage in product.ProductImages)
            {
                _context.ProductImages.Remove(productImage);
                productImage.Image.DeleteFile(_env.WebRootPath, "assets", "images", "content", "shop");
            }

            _context.Products.Remove(product);

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Update(int? id)
        {
            if (id is null || id <= 0) return BadRequest();

            Product? product = await _context.Products
                .Include(p => p.ProductImages)
                .Include(p => p.ProductFeatureValues)
                .ThenInclude(pfv => pfv.FeatureValue)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product is null) return NotFound();

            var features = await _context.Features
                .Include(f => f.FeatureValues)
                .ToListAsync();

            var featureSelections = features.Select(f =>
            {
                var selectedValues = product.ProductFeatureValues
                    .Where(pfv => pfv.FeatureValue.FeatureId == f.Id)
                    .Select(pfv => pfv.FeatureValueId)
                    .ToList();

                return new FeatureSelectionVM
                {
                    FeatureId = f.Id,
                    FeatureName = f.Name,
                    Values = f.FeatureValues,
                    SelectedValueIds = selectedValues
                };
            }).ToList();

            UpdateProductVM productVM = new UpdateProductVM
            {
                Id = product.Id,
                Name = product.Name,
                Price = product.Price,
                Description = product.Description,
                CategoryId = product.ProductCategoryId,
                PrimaryImage = product.ProductImages.FirstOrDefault(p => p.IsPrimary == true)?.Image,
                Categories = await _context.ProductCategories.ToListAsync(),
                Zodiacs = await _context.Zodiacs.ToListAsync(),
                FeatureSelections = featureSelections
            };

            return View(productVM);
        }

        [HttpPost]
        public async Task<IActionResult> Update(int? id, UpdateProductVM productVM)
        {
            productVM.Categories = await _context.ProductCategories.ToListAsync();
            productVM.Zodiacs = await _context.Zodiacs.ToListAsync();

            if (!ModelState.IsValid) return View(productVM);

            if (productVM.MainPhoto is not null)
            {
                if (!productVM.MainPhoto.ValidateType("image/"))
                {
                    ModelState.AddModelError(nameof(UpdateProductVM.MainPhoto), "file type is incorrect");
                    return View(productVM);
                }

                if (!productVM.MainPhoto.ValidateSize(FileSize.KB, 1000))
                {
                    ModelState.AddModelError(nameof(UpdateProductVM.MainPhoto), "file size must be less than 1000 KB");
                    return View(productVM);
                }
            }

            bool result = productVM.Categories.Any(c => c.Id == productVM.CategoryId);
            if (!result)
            {
                ModelState.AddModelError(nameof(UpdateProductVM.CategoryId), "Category does not exist!");
                return View(productVM);
            }

            bool nameResult = await _context.Products.AnyAsync(p => p.Name == productVM.Name && p.Id != id);
            if (nameResult)
            {
                ModelState.AddModelError(nameof(UpdateProductVM.Name), $"product: {productVM.Name} is already exist...");
                return View(productVM);
            }

            Product? existed = await _context.Products
                .Include(p => p.ProductImages)
                .Include(p => p.ProductFeatureValues)
                .Include(p => p.ProductZodiacs)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (existed is null) return NotFound();

            if (productVM.MainPhoto is not null)
            {
                ProductImages productImg = new ProductImages()
                {
                    Image = await productVM.MainPhoto.CreateFileAsync(_env.WebRootPath, "assets", "images", "content", "shop"),
                    IsPrimary = true,
                    CreatedAt = DateTime.Now
                };

                ProductImages existedMain = existed.ProductImages.FirstOrDefault(pi => pi.IsPrimary == true);
                if (existedMain != null)
                {
                    existedMain.Image.DeleteFile(_env.WebRootPath, "assets", "images", "content", "shop");
                    existed.ProductImages.Remove(existedMain);
                }
                existed.ProductImages.Add(productImg);
            }

            existed.Name = productVM.Name;
            existed.Price = productVM.Price.Value;
            existed.ProductCategoryId = productVM.CategoryId.Value;
            existed.Description = productVM.Description;

            _context.ProductZodiacs.RemoveRange(existed.ProductZodiacs);

            if (productVM.SelectedZodiacIds != null && productVM.SelectedZodiacIds.Any())
            {
                foreach (var zodiacId in productVM.SelectedZodiacIds)
                {
                    var productZodiac = new ProductZodiac
                    {
                        ProductId = existed.Id,
                        ZodiacId = zodiacId
                    };
                    await _context.ProductZodiacs.AddAsync(productZodiac);
                }
            }
            _context.ProductFeatureValues.RemoveRange(existed.ProductFeatureValues);

            if (productVM.FeatureSelections != null)
            {
                foreach (var featureSelection in productVM.FeatureSelections)
                {
                    if (featureSelection.SelectedValueIds != null)
                    {
                        foreach (var featureValueId in featureSelection.SelectedValueIds)
                        {
                            var newRelation = new ProductFeatureValue
                            {
                                ProductId = existed.Id,
                                FeatureValueId = featureValueId
                            };
                            await _context.ProductFeatureValues.AddAsync(newRelation);
                        }
                    }
                }
            }

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || id <= 0) return BadRequest();

            var product = await _context.Products
                .Include(p => p.ProductCategory)
                .Include(p => p.ProductImages)
                .Include(p => p.ProductFeatureValues)
                    .ThenInclude(pfv => pfv.FeatureValue)
                        .ThenInclude(fv => fv.Feature)
                .Include(p => p.ProductZodiacs)
                    .ThenInclude(pz => pz.Zodiac)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null) return NotFound();

            var features = product.ProductFeatureValues
                .GroupBy(pfv => pfv.FeatureValue.Feature)
                .Select(g => new
                {
                    FeatureName = g.Key.Name,
                    Values = g.Select(pfv => pfv.FeatureValue.Value).ToList()
                })
                .ToList();

            var zodiacs = product.ProductZodiacs.Select(pz => pz.Zodiac).ToList();
            var images = product.ProductImages.ToList();

            var model = new ProductDetailsVM
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                Discount = product.Discount,
                CategoryName = product.ProductCategory.Name,
                Images = images,
                Features = features.Select(f => new FeatureDisplayVM
                {
                    FeatureName = f.FeatureName,
                    Values = f.Values
                }).ToList(),
                ZodiacNames = zodiacs,
                Stock = product.Stock,
                SalesCount = product.SalesCount,
                ViewsCount = product.ViewsCount,
                Rating = product.Rating,
                ReviewCount = product.ReviewCount,
                FreeShipping = product.FreeShipping,
                CodAvailable = product.CodAvailable,
                ShippingDays = product.ShippingDays
            };

            return View(model);
        }


    }
}
