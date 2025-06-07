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
            Services = await _context.Services.ToListAsync(),
            ServiceCategories = await _context.ServiceCategories.ToListAsync(),
            AboutUs = await _context.AboutUs.ToListAsync(),
            Products = await _context.Products.ToListAsync(),
            ProductCategories = await _context.ProductCategories.ToListAsync(),
            Zodiacs = await _context.Zodiacs.ToListAsync(),
            ZodiacElements = await _context.ZodiacElements.ToListAsync(),
            News = await _context.News.ToListAsync()
        };

        return View(vm);
    }

}
