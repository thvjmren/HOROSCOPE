using HoroScope.DAL;
using HoroScope.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HoroScope.Controllers
{
    public class BlogController : Controller
    {
        private readonly AppDbContext _context;

        public BlogController(AppDbContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index(int? blogId)
        {
            var Blogs = blogId == null
            ? await _context.Blogs.Where(p => !p.IsDeleted).Include(p => p.BlogImages.Where(pi => pi.IsPrimary != null)).ToListAsync()
            : await _context.Blogs.Where(p => p.BlogCategoryId == blogId && !p.IsDeleted).Include(p => p.BlogImages.Where(pi => pi.IsPrimary != null)).ToListAsync();

            var recentNews = await _context.Blogs
                .Include(p => p.BlogImages.Where(pi => pi.IsPrimary != null))
                .Where(p => !p.IsDeleted)
                .OrderByDescending(p => p.Id)
            .Take(3)
            .ToListAsync();

            BlogVM vm = new()
            {
                BlogCategories = await _context.BlogCategories
                .Where(pc => pc.IsDeleted == false)
                .ToListAsync(),

                Blogs = Blogs,

                RecentNews = recentNews,

                BlogCount = Blogs.Count,
            };

            return View(vm);
        }
    }
}
