using HoroScope.DAL;
using HoroScope.Models;
using HoroScope.Utilities.Enums;
using HoroScope.Utilities.Extensions;
using HoroScope.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace HoroScope.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class BlogController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;

        public BlogController(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        public async Task<IActionResult> Index(int page = 1)
        {
            int count = await _context.Blogs.CountAsync();
            int pageSize = 3;
            int totalPage = (int)Math.Ceiling((double)count / pageSize);

            if (page < 1 || page > totalPage)
                return BadRequest();

            var blogs = await _context.Blogs
                .Where(b => b.IsDeleted == false)
                .Include(b => b.BlogCategory)
                .Include(b => b.AppUser)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(b => new GetBlogVM
                {
                    Id = b.Id,
                    Title = b.Title,
                    Content = b.Content,
                    Image = b.Image,
                    BlogCategoryName = b.BlogCategory.Name,
                    AuthorName = b.AppUser.UserName,
                    LikesCount = b.Likes.Count,
                    CommentsCount = b.Comments.Count,
                    CreatedDate = b.CreatedAt
                })
                .ToListAsync();

            var paginatedVM = new PaginatedVM<GetBlogVM>
            {
                TotalPage = totalPage,
                CurrentPage = page,
                Items = blogs
            };

            return View(paginatedVM);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return BadRequest();

            var blog = await _context.Blogs
                .Include(b => b.BlogCategory)
                .Include(b => b.AppUser)
                .Include(b => b.Likes)
                .Include(b => b.Comments)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (blog == null)
                return NotFound();

            var blogDetailVM = new GetBlogVM
            {
                Id = blog.Id,
                Title = blog.Title,
                Content = blog.Content,
                Image = blog.Image,
                BlogCategoryName = blog.BlogCategory.Name,
                AuthorName = blog.AppUser.UserName,
                LikesCount = blog.Likes.Count,
                CommentsCount = blog.Comments.Count,
                CreatedDate = blog.CreatedAt
            };

            return View(blogDetailVM);
        }

        public async Task<IActionResult> Create()
        {
            var categories = await _context.BlogCategories.ToListAsync();
            CreateBlogVM vm = new CreateBlogVM
            {
                BlogCategories = categories,
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateBlogVM blogVM)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            blogVM.BlogCategories = await _context.BlogCategories.ToListAsync();

            if (!ModelState.IsValid)
            {
                return View(blogVM);
            }

            bool result = blogVM.BlogCategories.Any(c => c.Id == blogVM.BlogCategoryId);
            if (!result)
            {
                ModelState.AddModelError(nameof(CreateBlogVM.BlogCategoryId), "category does not exist");
                return View(blogVM);
            }

            if (!blogVM.Image.ValidateType("image/"))
            {
                ModelState.AddModelError(nameof(CreateBlogVM.Image), "file type is incorrect");
                return View(blogVM);
            }

            if (!blogVM.Image.ValidateSize(FileSize.KB, 500))
            {
                ModelState.AddModelError(nameof(CreateBlogVM.Image), "file size must be less than 500 KB");
                return View(blogVM);
            }

            bool nameResult = await _context.Blogs.AnyAsync(p => p.Title == blogVM.Title);
            if (nameResult)
            {
                ModelState.AddModelError(nameof(blogVM.Title), $"this blog:{blogVM.Title} is already existed");
                return View(blogVM);
            }

            Blog blog = new Blog
            {
                Title = blogVM.Title,
                Content = blogVM.Content,
                Image = await blogVM.Image.CreateFileAsync(_env.WebRootPath, "assets", "images", "content", "blog"),
                BlogCategoryId = blogVM.BlogCategoryId,
                AppUserId = userId,
                CreatedAt = DateTime.UtcNow
            };

            await _context.Blogs.AddAsync(blog);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Update(int? id)
        {
            if (id == null)
            {
                return BadRequest();
            }

            var blog = await _context.Blogs
                .Include(b => b.BlogCategory)
                .Include(b => b.AppUser)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (blog == null)
            {
                return NotFound();
            }

            var categories = await _context.BlogCategories.ToListAsync();

            UpdateBlogVM blogVM = new UpdateBlogVM
            {
                Id = blog.Id,
                Title = blog.Title,
                Content = blog.Content,
                CategoryId = blog.BlogCategoryId,
                Categories = categories,
                Image = blog.Image,
            };

            return View(blogVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(int? id, UpdateBlogVM blogVM)
        {
            if (id is null || id <= 0) return BadRequest();

            blogVM.Categories = await _context.BlogCategories.ToListAsync();

            if (!ModelState.IsValid)
            {
                return View(blogVM);
            }

            var existingBlog = await _context.Blogs
                .FirstOrDefaultAsync(b => b.Id == id);

            if (existingBlog == null)
            {
                return NotFound("Blog not found.");
            }

            if (blogVM.Photo != null)
            {
                if (!blogVM.Photo.ValidateType("image/"))
                {
                    ModelState.AddModelError(nameof(UpdateBlogVM.Photo), "Invalid image type.");
                    return View(blogVM);
                }

                if (!blogVM.Photo.ValidateSize(FileSize.KB, 500))
                {
                    ModelState.AddModelError(nameof(UpdateBlogVM.Photo), "File size must be less than 500 KB.");
                    return View(blogVM);
                }

                var newImage = await blogVM.Photo.CreateFileAsync(_env.WebRootPath, "assets", "images", "content", "blog");

                if (!string.IsNullOrEmpty(existingBlog.Image))
                {
                    existingBlog.Image.DeleteFile(_env.WebRootPath, "assets", "images", "content", "blog");
                }

                existingBlog.Image = newImage;
            }

            if (existingBlog.BlogCategoryId != blogVM.CategoryId)
            {
                existingBlog.BlogCategoryId = blogVM.CategoryId.Value;
            }

            if (existingBlog.Title != blogVM.Title)
            {
                bool titleExists = await _context.Blogs.AnyAsync(b => b.Title == blogVM.Title && b.Id != id);
                if (titleExists)
                {
                    ModelState.AddModelError(nameof(UpdateBlogVM.Title), $"A blog with the title '{blogVM.Title}' already exists.");
                    return View(blogVM);
                }
            }

            existingBlog.Title = blogVM.Title;
            existingBlog.Content = blogVM.Content;

            _context.Blogs.Update(existingBlog);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id is null || id <= 0) return BadRequest();

            var blog = await _context.Blogs.FirstOrDefaultAsync(b => b.Id == id);

            if (blog == null)
            {
                return NotFound("Blog not found.");
            }

            blog.IsDeleted = true;
            _context.Blogs.Update(blog);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> HardDelete(int? id)
        {
            if (id is null || id <= 0) return BadRequest();

            var blog = await _context.Blogs.FirstOrDefaultAsync(b => b.Id == id);

            if (blog == null)
            {
                return NotFound("Blog not found.");
            }

            if (!string.IsNullOrEmpty(blog.Image))
            {
                blog.Image.DeleteFile(_env.WebRootPath, "assets", "images", "content", "blog");
            }

            _context.Blogs.Remove(blog);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Restore(int? id)
        {
            if (id is null || id <= 0) return BadRequest();

            var blog = await _context.Blogs.FirstOrDefaultAsync(b => b.Id == id && b.IsDeleted);

            if (blog == null)
            {
                return NotFound("Deleted Blog not found.");
            }

            blog.IsDeleted = false;
            _context.Blogs.Update(blog);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> DeletedBlogs(int page = 1)
        {
            var blogs = await _context.Blogs
                .Where(b => b.IsDeleted)
                .Include(b => b.BlogCategory)
                .Include(b => b.AppUser)
                .Select(b => new GetBlogVM
                {
                    Id = b.Id,
                    Title = b.Title,
                    Content = b.Content,
                    Image = b.Image,
                    BlogCategoryName = b.BlogCategory.Name,
                    AuthorName = b.AppUser.UserName,
                    LikesCount = b.Likes.Count,
                    CommentsCount = b.Comments.Count,
                    CreatedDate = b.CreatedAt
                })
                .ToListAsync();

            return View(blogs);
        }

    }
}
