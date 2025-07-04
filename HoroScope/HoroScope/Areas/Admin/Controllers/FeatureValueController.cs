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
    public class FeatureValueController : Controller
    {
        private readonly AppDbContext _context;

        public FeatureValueController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var featureValues = await _context.FeatureValues
                .Where(fv => fv.IsDeleted == false)
                                              .Include(fv => fv.Feature)
                                              .ToListAsync();

            return View(featureValues);
        }

        public IActionResult Create()
        {
            var features = _context.Features.ToList();
            var viewModel = new CreateFeatureValueVM
            {
                Features = features
            };
            return View(viewModel);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateFeatureValueVM model)
        {
            model.Features = await _context.Features.ToListAsync();
            if (ModelState.IsValid)
            {
                var featureValue = new FeatureValue
                {
                    Value = model.Value,
                    FeatureId = model.FeatureId,
                };

                _context.FeatureValues.Add(featureValue);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(model);
        }


        public async Task<IActionResult> Update(int id)
        {
            var featureValue = await _context.FeatureValues
                                              .Include(fv => fv.Feature)
                                              .FirstOrDefaultAsync(fv => fv.Id == id);

            if (featureValue == null)
            {
                return NotFound();
            }

            var viewModel = new UpdateFeatureValueVM
            {
                Id = featureValue.Id,
                Value = featureValue.Value,
                FeatureId = featureValue.FeatureId,
                Features = await _context.Features.ToListAsync()
            };

            return View(viewModel);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(int id, UpdateFeatureValueVM model)

        {
            if (!ModelState.IsValid)
            {
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    Console.WriteLine(error.ErrorMessage);
                }
                return View(model);
            }


            if (id != model.Id)
            {
                return NotFound();
            }

            var featureValue = await _context.FeatureValues.FindAsync(id);
            if (featureValue == null)
            {
                return NotFound();
            }

            featureValue.Value = model.Value;
            featureValue.FeatureId = model.FeatureId;

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int id)
        {
            var featureValue = await _context.FeatureValues.FindAsync(id);
            if (featureValue == null)
            {
                return NotFound();
            }
            featureValue.IsDeleted = true;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> HardDelete(int id)
        {
            var featureValue = await _context.FeatureValues.FindAsync(id);
            if (featureValue == null)
            {
                return NotFound();
            }

            _context.FeatureValues.Remove(featureValue);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Restore(int id)
        {
            var featureValue = await _context.FeatureValues
                                              .FirstOrDefaultAsync(fv => fv.Id == id && fv.IsDeleted);
            if (featureValue == null)
            {
                return NotFound();
            }

            featureValue.IsDeleted = false;
            _context.Update(featureValue);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> DeletedFeatureValues()
        {
            var featureValues = await _context.FeatureValues
                .Where(fv => fv.IsDeleted)
                .Include(fv => fv.Feature)
                .ToListAsync();

            return View(featureValues);
        }
    }
}