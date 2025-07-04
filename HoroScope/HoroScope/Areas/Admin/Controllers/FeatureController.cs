using HoroScope.DAL;
using HoroScope.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class FeatureController : Controller
{
    private readonly AppDbContext _context;

    public FeatureController(AppDbContext context)
    {
        _context = context;
    }


    public async Task<IActionResult> Index()
    {
        var features = await _context.Features
                                      .Include(f => f.ProductCategory)
                                      .Include(f => f.FeatureValues)
                                      .Where(f => !f.IsDeleted)
                                      .ToListAsync();

        var featureVMs = features.Select(f => new GetFeatureVM
        {
            Id = f.Id,
            Name = f.Name,
            ProductCategoryId = f.ProductCategoryId,
            ProductCategory = f.ProductCategory,
            FeatureValues = f.FeatureValues,
            CreatedAt = f.CreatedAt,
            IsDeleted = f.IsDeleted
        }).ToList();

        return View(featureVMs);
    }
}
