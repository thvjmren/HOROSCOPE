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
    public class ProductCategoryController : Controller
    {
        private readonly AppDbContext _context;

        public ProductCategoryController(AppDbContext context)
        {
            _context = context;
        }


        public async Task<IActionResult> Index()
        {
            var categories = await _context.ProductCategories
                .Where(c => !c.IsDeleted)
                .Include(c => c.Features)
                .ToListAsync();

            var viewModel = categories.Select(c => new GetProductCategoryVM
            {
                Id = c.Id,
                Name = c.Name,
                Features = c.Features.Select(f => new FeatureVM
                {
                    Id = f.Id,
                    FeatureName = f.Name
                }).ToList()
            }).ToList();

            return View(viewModel);
        }

        public async Task<IActionResult> Create()
        {
            var features = await _context.Features
                .Where(f => f.ProductCategoryId == null)
                .ToListAsync();
            ViewBag.Features = features;

            return View(new CreateProductCategoryVM());
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateProductCategoryVM model)
        {
            if (ModelState.IsValid)
            {
                var category = new ProductCategory
                {
                    Name = model.Name,
                };

                if (model.SelectedFeatures != null && model.SelectedFeatures.Any())
                {
                    var features = await _context.Features
                        .Where(f => model.SelectedFeatures.Contains(f.Id) && f.ProductCategoryId == null)
                        .ToListAsync();

                    category.Features = features;
                }

                _context.ProductCategories.Add(category);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            return View(model);
        }




        [HttpGet]
        public async Task<IActionResult> Update(int id)
        {
            var category = await _context.ProductCategories
                .FirstOrDefaultAsync(c => c.Id == id);

            if (category == null)
            {
                return NotFound();
            }

            var model = new UpdateProductCategoryVM
            {
                Id = category.Id,
                Name = category.Name
            };

            return View(model);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(int id, UpdateProductCategoryVM model)
        {
            if (id != model.Id)
            {
                return BadRequest();
            }

            if (ModelState.IsValid)
            {
                var category = await _context.ProductCategories
                    .FirstOrDefaultAsync(c => c.Id == id);

                if (category == null)
                {
                    return NotFound();
                }

                category.Name = model.Name;

                _context.Update(category);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            return View(model);
        }
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return BadRequest();

            var category = await _context.ProductCategories
                .Include(c => c.Features)
                .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);

            if (category == null)
            {
                return NotFound();
            }

            category.IsDeleted = true;

            SoftDeleteFeatures(category.Features);

            _context.ProductCategories.Update(category);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        private void SoftDeleteFeatures(List<Feature> features)
        {
            if (features != null)
            {
                foreach (var feature in features)
                {
                    feature.IsDeleted = true;
                }
                _context.Features.UpdateRange(features);
            }
        }

        public async Task<IActionResult> Restore(int id)
        {
            var category = await _context.ProductCategories
                .FirstOrDefaultAsync(c => c.Id == id && c.IsDeleted == true);

            if (category == null)
            {
                return NotFound();
            }

            category.IsDeleted = false;

            RestoreFeatures(category.Features);

            _context.ProductCategories.Update(category);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(DeletedCategories));
        }

        private void RestoreFeatures(List<Feature> features)
        {
            if (features != null)
            {
                foreach (var feature in features)
                {
                    feature.IsDeleted = false;
                }
                _context.Features.UpdateRange(features);
            }
        }

        public async Task<IActionResult> DeletedCategories()
        {
            var deletedCategories = await _context.ProductCategories
                .Where(c => c.IsDeleted == true)
                .Include(c => c.Features)
                .ToListAsync();

            return View(deletedCategories);
        }
        public async Task<IActionResult> HardDelete(int? id)
        {
            if (id == null) return BadRequest();
            var category = await _context.ProductCategories
                .FirstOrDefaultAsync(c => c.Id == id && c.IsDeleted == true);

            if (category == null)
            {
                return NotFound();
            }

            var features = await _context.Features
                .Where(f => f.ProductCategoryId == category.Id)
                .ToListAsync();

            _context.Features.RemoveRange(features);

            _context.ProductCategories.Remove(category);

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(DeletedCategories));
        }

        public async Task<IActionResult> Details(int id)
        {
            var category = await _context.ProductCategories
                .Include(c => c.Features)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (category == null)
            {
                return NotFound();
            }

            var viewModel = new GetProductCategoryVM
            {
                Id = category.Id,
                Name = category.Name,
                Features = category.Features.Select(f => new FeatureVM
                {
                    Id = f.Id,
                    FeatureName = f.Name
                }).ToList(),
                CreatedAt = category.CreatedAt,
                IsDeleted = category.IsDeleted
            };

            return View(viewModel);
        }
    }
}