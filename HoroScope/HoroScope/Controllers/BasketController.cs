using HoroScope.DAL;
using HoroScope.Interfaces;
using HoroScope.Models;
using HoroScope.Utilities.Enums;
using HoroScope.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Security.Claims;

namespace HoroScope.Controllers
{
    public class BasketController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly IBasketService _basketService;

        public BasketController(AppDbContext context, UserManager<AppUser> userManager, IBasketService basketService)
        {
            _context = context;
            _userManager = userManager;
            _basketService = basketService;
        }
        public async Task<IActionResult> Index()
        {
            return View(await _basketService.GetBasketAsync());
        }

        [HttpPost]
        public async Task<JsonResult> AddBasket(int? id)
        {
            if (id == null || id <= 0)
                return Json(new { success = false, message = "Invalid product ID." });

            bool productExists = await _context.Products.AnyAsync(p => p.Id == id);
            if (!productExists)
                return Json(new { success = false, message = "Product not found." });

            if (User.Identity.IsAuthenticated)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                AppUser? user = await _userManager.Users
                    .Include(u => u.BasketItems)
                    .FirstOrDefaultAsync(u => u.Id == userId);

                if (user == null)
                    return Json(new { success = false, message = "User not found." });

                if (user.BasketItems == null)
                    user.BasketItems = new List<BasketItem>();

                var basketItem = user.BasketItems.FirstOrDefault(bi => bi.ProductId == id);

                if (basketItem == null)
                {
                    user.BasketItems.Add(new BasketItem
                    {
                        ProductId = id.Value,
                        Count = 1
                    });
                }
                else
                {
                    basketItem.Count++;
                }

                var changes = await _context.SaveChangesAsync();

                if (changes == 0)
                    return Json(new { success = false, message = "Failed to add item to basket." });

                int totalCount = user.BasketItems.Sum(bi => bi.Count);

                return Json(new { success = true, message = "Product added to basket.", count = totalCount });
            }
            else
            {
                List<BasketCookieItemVM> basketCookies;
                string cookieBasket = Request.Cookies["basket"];

                if (!string.IsNullOrEmpty(cookieBasket))
                {
                    basketCookies = JsonConvert.DeserializeObject<List<BasketCookieItemVM>>(cookieBasket);
                    var existed = basketCookies.FirstOrDefault(bc => bc.Id == id);

                    if (existed != null)
                    {
                        existed.Count++;
                    }
                    else
                    {
                        basketCookies.Add(new BasketCookieItemVM { Id = id.Value, Count = 1 });
                    }
                }
                else
                {
                    basketCookies = new List<BasketCookieItemVM> { new BasketCookieItemVM { Id = id.Value, Count = 1 } };
                }

                string updatedBasket = JsonConvert.SerializeObject(basketCookies);
                Response.Cookies.Append("basket", updatedBasket);

                int totalCount = basketCookies.Sum(bc => bc.Count);

                return Json(new { success = true, message = "Product added to basket.", count = totalCount });
            }
        }



        public async Task<IActionResult> RemoveFromBasket(int? id)
        {
            if (id == null || id <= 0)
                return BadRequest();

            if (User.Identity.IsAuthenticated)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var basketItem = await _context.BasketItems
                    .FirstOrDefaultAsync(bi => bi.AppUserId == userId && bi.ProductId == id);

                if (basketItem != null)
                {
                    _context.BasketItems.Remove(basketItem);
                    await _context.SaveChangesAsync();
                }
            }
            else
            {
                string basketCookie = Request.Cookies["basket"];
                if (string.IsNullOrEmpty(basketCookie))
                    return RedirectToAction(nameof(Index));

                var basketItems = JsonConvert.DeserializeObject<List<BasketCookieItemVM>>(basketCookie);

                var itemToRemove = basketItems.FirstOrDefault(x => x.Id == id);
                if (itemToRemove != null)
                {
                    basketItems.Remove(itemToRemove);

                    var cookieOptions = new CookieOptions
                    {
                        Expires = DateTimeOffset.Now.AddDays(7),
                        HttpOnly = false,
                        IsEssential = true,
                        Path = "/"
                    };

                    if (basketItems.Count > 0)
                    {
                        string updatedBasket = JsonConvert.SerializeObject(basketItems);
                        Response.Cookies.Append("basket", updatedBasket, cookieOptions);
                    }
                    else
                    {
                        Response.Cookies.Delete("basket");
                    }
                }
            }

            return RedirectToAction(nameof(Index));
        }


        public async Task<IActionResult> IncreaseCount(int? id)
        {
            if (id == null || id <= 0) return BadRequest();

            if (User.Identity.IsAuthenticated)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var basketItem = await _context.BasketItems.FirstOrDefaultAsync(b => b.AppUserId == userId && b.ProductId == id);

                if (basketItem != null)
                {
                    basketItem.Count++;
                    await _context.SaveChangesAsync();
                }
            }
            else
            {
                string basketCookie = Request.Cookies["basket"];
                if (string.IsNullOrEmpty(basketCookie)) return RedirectToAction(nameof(Index), "Basket");

                var basketItems = JsonConvert.DeserializeObject<List<BasketCookieItemVM>>(basketCookie);
                var item = basketItems.FirstOrDefault(x => x.Id == id);

                if (item != null)
                {
                    item.Count++;
                    string updatedCookie = JsonConvert.SerializeObject(basketItems);
                    Response.Cookies.Append("basket", updatedCookie, new CookieOptions
                    {
                        Expires = DateTimeOffset.Now.AddDays(7),
                        HttpOnly = true,
                        IsEssential = true,
                        Path = "/"
                    });
                }
            }

            return RedirectToAction(nameof(Index), "Basket");
        }

        public async Task<IActionResult> DecreaseCount(int? id)
        {
            if (id == null || id <= 0) return BadRequest();

            if (User.Identity.IsAuthenticated)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var basketItem = await _context.BasketItems.FirstOrDefaultAsync(b => b.AppUserId == userId && b.ProductId == id);

                if (basketItem != null)
                {
                    if (basketItem.Count > 1)
                        basketItem.Count--;
                    else
                        _context.BasketItems.Remove(basketItem);

                    await _context.SaveChangesAsync();
                }
            }
            else
            {
                string basketCookie = Request.Cookies["basket"];
                if (string.IsNullOrEmpty(basketCookie)) return RedirectToAction(nameof(Index), "Basket");

                var basketItems = JsonConvert.DeserializeObject<List<BasketCookieItemVM>>(basketCookie);
                var item = basketItems.FirstOrDefault(x => x.Id == id);

                if (item != null)
                {
                    if (item.Count > 1)
                        item.Count--;
                    else
                        basketItems.Remove(item);

                    if (basketItems.Count > 0)
                    {
                        string updatedCookie = JsonConvert.SerializeObject(basketItems);
                        Response.Cookies.Append("basket", updatedCookie, new CookieOptions
                        {
                            Expires = DateTimeOffset.Now.AddDays(7),
                            HttpOnly = true,
                            IsEssential = true,
                            Path = "/"
                        });
                    }
                    else
                    {
                        Response.Cookies.Delete("basket");
                    }
                }
            }

            return RedirectToAction(nameof(Index), "Basket");
        }



        [Authorize(Roles = "Member")]
        public async Task<IActionResult> CheckOut()
        {
            OrderVM vm = new()
            {
                BasketInOrderVMs = await _context.BasketItems.Where(bi => bi.AppUserId == User.FindFirstValue(ClaimTypes.NameIdentifier))
                .Select(bi => new BasketInOrderVM
                {
                    Count = bi.Count,
                    Name = bi.Product.Name,
                    Price = bi.Product.Price,
                    SubTotal = bi.Product.Price * bi.Count,
                }).ToListAsync()
            };

            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> CheckOut(OrderVM orderVM)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var basketItems = await _context.BasketItems
                .Where(bi => bi.AppUserId == userId)
                .Include(bi => bi.Product)
                .ToListAsync();

            if (!ModelState.IsValid)
            {
                orderVM.BasketInOrderVMs = basketItems.Select(bi => new BasketInOrderVM
                {
                    Count = bi.Count,
                    Name = bi.Product.Name,
                    Price = bi.Product.Price,
                    SubTotal = bi.Product.Price * bi.Count,
                }).ToList();
                return View(orderVM);
            }

            Order order = new()
            {
                Address = orderVM.Address,
                PhoneNumber = orderVM.PhoneNumber,
                Status = OrderStatus.Pending,
                CreatedAt = DateTime.Now,
                UserId = userId,
                IsDeleted = false,
                OrderItems = basketItems.Select(bi => new OrderItem
                {
                    ProductId = bi.ProductId,
                    Quantity = bi.Count,
                    UnitPrice = bi.Product.Price
                }).ToList()
            };

            await _context.Orders.AddAsync(order);

            _context.BasketItems.RemoveRange(basketItems);

            await _context.SaveChangesAsync();

            return RedirectToAction("OrderConfirmation", new { id = order.Id });
        }

        public async Task<IActionResult> OrderConfirmation(int id)
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
                return NotFound();

            return View(order);
        }

    }
}