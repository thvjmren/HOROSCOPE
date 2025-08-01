﻿using HoroScope.DAL;
using HoroScope.Models;
using HoroScope.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HoroScope.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class ServiceCategoryController : Controller
    {
        private readonly AppDbContext _context;

        public ServiceCategoryController(AppDbContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index()
        {

            List<GetServiceCategoryVM> categoryVMs = await _context.ServiceCategories
                .Where(sc => !sc.IsDeleted)
                .Select(sc => new GetServiceCategoryVM
                {
                    Id = sc.Id,
                    Name = sc.Name,
                    CreatedAt = sc.CreatedAt,
                    IsDeleted = sc.IsDeleted,
                }).ToListAsync();

            return View(categoryVMs);
        }


        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateServiceCategoryVM category)
        {
            if (!ModelState.IsValid)
            {
                return View(category);
            }

            bool result = await _context.ServiceCategories.AnyAsync(cs => cs.Name == category.Name);
            if (result)
            {
                ModelState.AddModelError(nameof(CreateServiceCategoryVM.Name), $"This Category: {category.Name} already exists");
                return View(category);
            }

            ServiceCategory serviceCategory = new()
            {
                Name = category.Name,
                CreatedAt = DateTime.Now
            };

            await _context.ServiceCategories.AddAsync(serviceCategory);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id is null) return BadRequest();

            ServiceCategory? category = await _context.ServiceCategories
                .FirstOrDefaultAsync(sc => sc.Id == id && sc.IsDeleted == false);

            if (category is null) return NotFound();
            category.IsDeleted = true;
            _context.ServiceCategories.Update(category);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> HardDelete(int? id)
        {
            if (id is null) return BadRequest();

            ServiceCategory? category = await _context.ServiceCategories
                .FirstOrDefaultAsync(sc => sc.Id == id);

            if (category is null) return NotFound();

            _context.ServiceCategories.Remove(category);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Update(int? id)
        {
            if (id is null || id <= 0) return BadRequest();

            ServiceCategory? category = await _context.ServiceCategories.FirstOrDefaultAsync(sc => sc.Id == id);

            if (category is null) return NotFound();

            UpdateServiceCategoryVM categoryVM = new()
            {
                Name = category.Name,
            };

            return View(categoryVM);
        }

        [HttpPost]
        public async Task<IActionResult> Update(int? id, UpdateServiceCategoryVM? categoryVM)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            bool result = await _context.ServiceCategories.AnyAsync(c => c.Name == categoryVM.Name && c.Id != id);
            if (result)
            {
                ModelState.AddModelError(nameof(UpdateServiceCategoryVM.Name), $"This Category: {categoryVM.Name} is already exists");
                return View(categoryVM);
            }

            ServiceCategory? existed = await _context.ServiceCategories.FirstOrDefaultAsync(c => c.Id == id);

            if (existed.Name == categoryVM.Name) return RedirectToAction(nameof(Index));

            existed.Name = categoryVM.Name;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Restore(int? id)
        {
            if (id is null) return BadRequest();

            ServiceCategory? category = await _context.ServiceCategories
                .FirstOrDefaultAsync(sc => sc.Id == id && sc.IsDeleted == true);

            if (category is null) return NotFound();

            category.IsDeleted = false;
            _context.ServiceCategories.Update(category);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(DeletedCategories));
        }
        public async Task<IActionResult> DeletedCategories()
        {
            var deletedCategories = await _context.ServiceCategories
                .Where(sc => sc.IsDeleted == true)
                .ToListAsync();

            return View(deletedCategories);
        }
    }
}
