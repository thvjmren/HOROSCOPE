using HoroScope.DAL;
using HoroScope.Models;
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
        public async Task<IActionResult> Index(int? categoryId, int page = 1)
        {
            int pageSize = 3;

            var query = _context.Blogs.Where(b => !b.IsDeleted);

            if (categoryId != null)
            {
                query = query.Where(b => b.BlogCategoryId == categoryId);
            }

            int count = await query.CountAsync();

            double total = Math.Ceiling((double)count / pageSize);

            if (page > total && total != 0) return BadRequest();

            var Blogs = await query
                .OrderByDescending(b => b.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var recentNews = await _context.Blogs
                .Where(b => !b.IsDeleted)
                .OrderByDescending(b => b.Id)
                .Take(3)
                .ToListAsync();

            BlogVM vm = new()
            {
                BlogCategories = await _context.BlogCategories
                    .Where(bc => !bc.IsDeleted)
                    .ToListAsync(),


                Blogs = new PaginatedVM<Blog>
                {
                    Items = Blogs,
                    CurrentPage = page,
                    TotalPage = total
                },
                RecentNews = recentNews,
                BlogCount = count,
                SelectedCategoryId = categoryId
            };

            return View(vm);
        }


        public async Task<IActionResult> Details(int? id)
        {
            if (id is null || id <= 0) return BadRequest();

            var blog = await _context.Blogs.FirstOrDefaultAsync(b => b.Id == id && !b.IsDeleted);

            if (blog is null) return NotFound();

            var recentNews = await _context.Blogs
                .Where(b => !b.IsDeleted)
                .OrderByDescending(b => b.Id)
                .Take(3)
                .ToListAsync();

            BlogDetailsVM vm = new BlogDetailsVM
            {
                BlogCategories = await _context.BlogCategories
                .Where(bc => bc.IsDeleted == false)
                .ToListAsync(),

                RecentNews = recentNews,

                Blog = blog
            };

            return View(vm);
        }
    }
}
