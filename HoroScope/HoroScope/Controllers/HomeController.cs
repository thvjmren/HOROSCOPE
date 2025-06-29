using HoroScope.DAL;
using HoroScope.Interfaces;
using HoroScope.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HoroScope.Controllers;

public class HomeController : Controller
{
    private readonly AppDbContext _context;
    private readonly IEmailService _service;

    public HomeController(AppDbContext context, IEmailService service)
    {
        _context = context;
        _service = service;
    }

    public async Task<IActionResult> Index(int? categoryId)
    {
        ViewData["SelectedCategoryId"] = categoryId;

        var services = await _context.Services
            .Where(s => !s.IsDeleted && (categoryId == null || s.ServiceCategoryId == categoryId))
            .ToListAsync();

        var ServiceCategories = await _context.ServiceCategories
            .Where(p => !p.IsDeleted)
            .OrderByDescending(sc => sc.Services.Count(s => !s.IsDeleted))
            .Take(4)
            .ToListAsync();


        HomeVM vm = new()
        {
            Services = services,
            ServiceCategories = ServiceCategories,
            AboutUs = await _context.AboutUs.Where(a => !a.IsDeleted).ToListAsync(),

            Products = await _context.Products
                .Include(p => p.ProductImages.Where(pi => pi.IsPrimary != null))
                .Where(p => !p.IsDeleted)
                .OrderByDescending(p => p.Id)
                .Take(3)
                .ToListAsync(),

            ProductCategories = await _context.ProductCategories.Where(pc => !pc.IsDeleted).ToListAsync(),
            Zodiacs = await _context.Zodiacs.Where(z => !z.IsDeleted).ToListAsync(),
            ZodiacElements = await _context.ZodiacElements.Where(ze => !ze.IsDeleted).ToListAsync(),
            Blogs = await _context.Blogs.Where(b => !b.IsDeleted).Include(b => b.Likes).Include(b => b.Comments).Include(b => b.AppUser).ToListAsync(),
            Partners = await _context.Partners.Where(p => !p.IsDeleted).ToListAsync(),
            Experts = await _context.Experts.Where(e => !e.IsDeleted).ToListAsync(),
        };

        return View(vm);
    }

    public IActionResult Test()
    {
        Response.Cookies.Append("name", "sunay", new CookieOptions { MaxAge = TimeSpan.FromSeconds(10) });


        HttpContext.Session.SetString("name2", "rena");

        return Ok();
    }

    public IActionResult GetCookie()
    {
        return Content(Request.Cookies["name"]);
    }

}
