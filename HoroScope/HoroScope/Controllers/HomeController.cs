using HoroScope.DAL;
using HoroScope.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HoroScope.Controllers;

public class HomeController : Controller
{
    private readonly AppDbContext _context;

    public HomeController(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        HomeVM vm = new()
        {
            Services = await _context.Services.Where(s => !s.IsDeleted).ToListAsync(),
            ServiceCategories = await _context.ServiceCategories.Where(sc => !sc.IsDeleted).ToListAsync(),
            AboutUs = await _context.AboutUs.Where(a => !a.IsDeleted).ToListAsync(),
            Products = await _context.Products.Where(p => !p.IsDeleted).ToListAsync(),
            ProductCategories = await _context.ProductCategories.Where(pc => !pc.IsDeleted).ToListAsync(),
            Zodiacs = await _context.Zodiacs.Where(z => !z.IsDeleted).ToListAsync(),
            ZodiacElements = await _context.ZodiacElements.Where(ze => !ze.IsDeleted).ToListAsync(),
            News = await _context.News.Where(n => !n.IsDeleted).ToListAsync(),
            Partners = await _context.Partners.Where(p => !p.IsDeleted).ToListAsync(),
            Experts = await _context.Experts.Where(e => !e.IsDeleted).ToListAsync(),
        };

        return View(vm);
    }

}
