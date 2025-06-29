using HoroScope.DAL;
using HoroScope.Models;
using HoroScope.ViewModels;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HoroScope.Controllers
{
    public class BlogController : Controller
    {
        private readonly AppDbContext _context;

        private readonly UserManager<AppUser> _userManager;
        private readonly IAntiforgery _antiforgery;

        public BlogController(AppDbContext context, UserManager<AppUser> userManager, IAntiforgery antiforgery)
        {
            _context = context;
            _userManager = userManager;
            _antiforgery = antiforgery;
        }

        public async Task<IActionResult> Index(string? search, int? categoryId, int page = 1, int? year = null, int? month = null)
        {
            int pageSize = 3;

            var query = _context.Blogs.Where(b => !b.IsDeleted);

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(b => b.Title.ToLower().Contains(search.ToLower()));
            }

            if (categoryId != null)
            {
                query = query.Where(b => b.BlogCategoryId == categoryId);
            }

            if (year != null && month != null)
            {
                query = query.Where(b => b.CreatedAt.Year == year && b.CreatedAt.Month == month);
            }

            int count = await query.CountAsync();

            double total = Math.Ceiling((double)count / pageSize);

            if (page > total && total != 0) return BadRequest();

            var Blogs = await query
                .Include(b => b.AppUser)
                .OrderByDescending(b => b.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(b => new Blog
                {
                    Id = b.Id,
                    Title = b.Title,
                    Content = b.Content,
                    CreatedAt = b.CreatedAt,
                    Image = b.Image,
                    BlogCategoryId = b.BlogCategoryId,
                    LikesCount = _context.BlogLikes.Count(l => l.BlogId == b.Id),
                    CommentsCount = _context.BlogComments.Count(c => c.BlogId == b.Id),
                    AppUser = b.AppUser
                })
                .ToListAsync();


            var recentNews = await _context.Blogs
                .Where(b => !b.IsDeleted)
                .OrderByDescending(b => b.Id)
                .Take(3)
                .ToListAsync();

            var archives = await _context.Blogs
                .Where(b => !b.IsDeleted)
                .GroupBy(b => new { b.CreatedAt.Year, b.CreatedAt.Month, b.CreatedAt.Day })
                .Select(g => new ArchiveVM
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    Count = g.Count()
                }).OrderByDescending(x => x.Year)
                .ThenByDescending(x => x.Month)
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
                SelectedCategoryId = categoryId,
                Archives = archives,
                SelectedYear = year,
                SelectedMonth = month
            };

            return View(vm);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id is null || id <= 0) return BadRequest();

            var blog = await _context.Blogs.FirstOrDefaultAsync(b => b.Id == id && !b.IsDeleted);
            if (blog is null) return NotFound();

            var user = User.Identity.IsAuthenticated ? await _userManager.GetUserAsync(User) : null;

            var likesCount = await _context.BlogLikes.CountAsync(l => l.BlogId == id);

            var userHasLiked = false;
            if (user != null)
            {
                userHasLiked = await _context.BlogLikes.AnyAsync(l => l.BlogId == id && l.AppUserId == user.Id);
            }

            var tokens = _antiforgery.GetAndStoreTokens(HttpContext);
            ViewData["RequestVerificationToken"] = tokens.RequestToken;

            BlogDetailsVM vm = new BlogDetailsVM
            {
                BlogCategories = await _context.BlogCategories.Where(bc => !bc.IsDeleted).ToListAsync(),
                RecentNews = await _context.Blogs.Where(b => !b.IsDeleted).OrderByDescending(b => b.Id).Take(3).ToListAsync(),
                Blog = blog,
                BlogComments = await _context.BlogComments
                .Include(c => c.AppUser)
                .Where(c => c.BlogId == id)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync(),

                BlogComment = new BlogComment(),
                Archives = await _context.Blogs
                                    .Where(b => !b.IsDeleted)
                                    .GroupBy(b => new { b.CreatedAt.Year, b.CreatedAt.Month })
                                    .Select(g => new ArchiveVM
                                    {
                                        Year = g.Key.Year,
                                        Month = g.Key.Month,
                                        Count = g.Count()
                                    })
                                    .OrderByDescending(x => x.Year)
                                    .ThenByDescending(x => x.Month)
                                    .ToListAsync(),
                BlogLikesCount = likesCount,
                UserHasLiked = userHasLiked
            };

            return View(vm);
        }


        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddComment(BlogDetailsVM blogComment)
        {
            var user = User.Identity.IsAuthenticated
                ? await _userManager.GetUserAsync(User)
                : null;

            BlogComment newBlogComment = new()
            {
                Text = blogComment.BlogCommentVM.Comment,
                AppUserId = user?.Id,
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false,
                BlogId = blogComment.BlogCommentVM.BlogId,
                Email = user?.Email
            };

            _context.BlogComments.Add(newBlogComment);
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", new { id = blogComment.BlogCommentVM.BlogId });
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleLike(int blogId)
        {
            if (!User.Identity.IsAuthenticated)
                return Unauthorized();

            var user = await _userManager.GetUserAsync(User);

            var existingLike = await _context.BlogLikes
                .FirstOrDefaultAsync(l => l.BlogId == blogId && l.AppUserId == user.Id);

            bool userHasLiked;

            if (existingLike != null)
            {
                _context.BlogLikes.Remove(existingLike);
                userHasLiked = false;
            }
            else
            {
                var like = new BlogLike
                {
                    BlogId = blogId,
                    AppUserId = user.Id,
                };
                await _context.BlogLikes.AddAsync(like);
                userHasLiked = true;
            }

            await _context.SaveChangesAsync();

            int likesCount = await _context.BlogLikes.CountAsync(l => l.BlogId == blogId);

            return Json(new { likesCount, userHasLiked });
        }

    }
}
