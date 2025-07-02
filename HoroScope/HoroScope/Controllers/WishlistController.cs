using HoroScope.DAL;
using HoroScope.Models;
using HoroScope.ViewModels;
using HoroScope.ViewModels.Wishlist;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Security.Claims;

namespace HoroScope.Controllers
{
    [Authorize(Roles = "Member")]
    public class WishlistController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        public WishlistController(AppDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> AddToWishlist(int? id)
        {
            if (id == null || id <= 0) return BadRequest();

            bool exists = await _context.Products.AnyAsync(p => p.Id == id);
            if (!exists) return NotFound();

            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }

            AppUser user = await _userManager.Users
                .Include(u => u.WishlistItems)
                .FirstOrDefaultAsync(u => u.Id == User.FindFirstValue(ClaimTypes.NameIdentifier));

            if (user == null) return Unauthorized();

            bool alreadyExists = user.WishlistItems.Any(w => w.ProductId == id);
            if (!alreadyExists)
            {
                user.WishlistItems.Add(new WishlistItem
                {
                    ProductId = id.Value
                });
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index), "Shop");
        }


        public async Task<IActionResult> Index()
        {
            List<WishlistItemVM> wishlistVM = new();

            if (User.Identity.IsAuthenticated)
            {
                AppUser user = await _userManager.Users
                    .Include(u => u.WishlistItems)
                    .ThenInclude(w => w.Product)
                    .ThenInclude(p => p.ProductImages)
                    .FirstOrDefaultAsync(u => u.Id == User.FindFirstValue(ClaimTypes.NameIdentifier));

                if (user != null)
                {
                    wishlistVM = user.WishlistItems.Select(w => new WishlistItemVM
                    {
                        Id = w.Product.Id,
                        Name = w.Product.Name,
                        Price = w.Product.Price,
                        Image = w.Product.ProductImages.FirstOrDefault(pi => pi.IsPrimary == true)?.Image ?? "default.jpg"
                    }).ToList();
                }
            }
            else
            {
                string cookie = Request.Cookies["wishlist"];
                if (!string.IsNullOrEmpty(cookie))
                {
                    var cookieItems = JsonConvert.DeserializeObject<List<WishlistCookieItemVM>>(cookie);

                    foreach (var item in cookieItems)
                    {
                        var product = await _context.Products
                            .Include(p => p.ProductImages)
                            .FirstOrDefaultAsync(p => p.Id == item.Id);

                        if (product != null)
                        {
                            wishlistVM.Add(new WishlistItemVM
                            {
                                Id = product.Id,
                                Name = product.Name,
                                Price = product.Price,
                                Image = product.ProductImages.FirstOrDefault(pi => pi.IsPrimary == true)?.Image ?? "default.jpg"
                            });
                        }
                    }
                }
            }

            return View(wishlistVM);
        }

        public async Task<IActionResult> Remove(int? id)
        {
            if (id == null || id <= 0) return BadRequest();

            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }

            AppUser user = await _userManager.Users
                .Include(u => u.WishlistItems)
                .FirstOrDefaultAsync(u => u.Id == User.FindFirstValue(ClaimTypes.NameIdentifier));

            var item = user.WishlistItems.FirstOrDefault(w => w.ProductId == id);
            if (item != null)
            {
                user.WishlistItems.Remove(item);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

    }
}
