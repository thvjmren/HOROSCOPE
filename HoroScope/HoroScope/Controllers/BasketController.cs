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
using Stripe.Checkout;
using System.Security.Claims;
using AppOrder = HoroScope.Models.Order;

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
            var basket = await _basketService.GetBasketAsync();
            if (basket == null || !basket.Any())
            {
                basket = new List<BasketItemVM>();
            }
            return View(basket);
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
                var result = await AddBasketForUser(id.Value);
                return Json(result);
            }
            else
            {
                var result = AddBasketForGuest(id.Value);
                return Json(result);
            }
        }

        private async Task<object> AddBasketForUser(int productId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.Users
                .Include(u => u.BasketItems)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
                return new { success = false, message = "User not found." };

            user.BasketItems ??= new List<BasketItem>();

            var basketItem = user.BasketItems.FirstOrDefault(bi => bi.ProductId == productId);

            if (basketItem == null)
                user.BasketItems.Add(new BasketItem { ProductId = productId, Count = 1 });
            else
                basketItem.Count++;

            int changes = await _context.SaveChangesAsync();

            if (changes == 0)
                return new { success = false, message = "Failed to add item to basket." };

            int totalCount = user.BasketItems.Sum(bi => bi.Count);
            return new { success = true, message = "Product added to basket.", count = totalCount };
        }

        private object AddBasketForGuest(int productId)
        {
            var cookieBasket = Request.Cookies["basket"];
            List<BasketCookieItemVM> basketCookies = !string.IsNullOrEmpty(cookieBasket)
                ? JsonConvert.DeserializeObject<List<BasketCookieItemVM>>(cookieBasket)
                : new List<BasketCookieItemVM>();

            var existed = basketCookies.FirstOrDefault(bc => bc.Id == productId);
            if (existed != null)
                existed.Count++;
            else
                basketCookies.Add(new BasketCookieItemVM { Id = productId, Count = 1 });

            string updatedBasket = JsonConvert.SerializeObject(basketCookies);
            Response.Cookies.Append("basket", updatedBasket, new Microsoft.AspNetCore.Http.CookieOptions
            {
                Expires = DateTimeOffset.Now.AddDays(7),
                HttpOnly = true,
                IsEssential = true,
                Path = "/"
            });

            int totalCount = basketCookies.Sum(bc => bc.Count);
            return new { success = true, message = "Product added to basket.", count = totalCount };
        }

        public async Task<IActionResult> RemoveFromBasket(int? id)
        {
            if (id == null || id <= 0)
                return BadRequest();

            if (User.Identity.IsAuthenticated)
                await RemoveFromUserBasket(id.Value);
            else
                RemoveFromGuestBasket(id.Value);

            return RedirectToAction(nameof(Index));
        }

        private async Task RemoveFromUserBasket(int productId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var basketItem = await _context.BasketItems
                .FirstOrDefaultAsync(bi => bi.AppUserId == userId && bi.ProductId == productId);

            if (basketItem != null)
            {
                _context.BasketItems.Remove(basketItem);
                await _context.SaveChangesAsync();
            }
        }

        private void RemoveFromGuestBasket(int productId)
        {
            var basketCookie = Request.Cookies["basket"];
            if (string.IsNullOrEmpty(basketCookie))
                return;

            var basketItems = JsonConvert.DeserializeObject<List<BasketCookieItemVM>>(basketCookie);
            var itemToRemove = basketItems.FirstOrDefault(x => x.Id == productId);

            if (itemToRemove != null)
            {
                basketItems.Remove(itemToRemove);

                var cookieOptions = new Microsoft.AspNetCore.Http.CookieOptions
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

        public async Task<IActionResult> IncreaseCount(int? id)
        {
            if (id == null || id <= 0)
                return BadRequest();

            if (User.Identity.IsAuthenticated)
                await ChangeUserBasketItemCount(id.Value, increase: true);
            else
                ChangeGuestBasketItemCount(id.Value, increase: true);

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> DecreaseCount(int? id)
        {
            if (id == null || id <= 0)
                return BadRequest();

            if (User.Identity.IsAuthenticated)
                await ChangeUserBasketItemCount(id.Value, increase: false);
            else
                ChangeGuestBasketItemCount(id.Value, increase: false);

            return RedirectToAction(nameof(Index));
        }

        private async Task ChangeUserBasketItemCount(int productId, bool increase)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var basketItem = await _context.BasketItems.FirstOrDefaultAsync(b => b.AppUserId == userId && b.ProductId == productId);

            if (basketItem != null)
            {
                if (increase)
                    basketItem.Count++;
                else
                {
                    if (basketItem.Count > 1)
                        basketItem.Count--;
                    else
                        _context.BasketItems.Remove(basketItem);
                }
                await _context.SaveChangesAsync();
            }
        }

        private void ChangeGuestBasketItemCount(int productId, bool increase)
        {
            var basketCookie = Request.Cookies["basket"];
            if (string.IsNullOrEmpty(basketCookie))
                return;

            var basketItems = JsonConvert.DeserializeObject<List<BasketCookieItemVM>>(basketCookie);
            var item = basketItems.FirstOrDefault(x => x.Id == productId);

            if (item != null)
            {
                if (increase)
                {
                    item.Count++;
                }
                else
                {
                    if (item.Count > 1)
                        item.Count--;
                    else
                        basketItems.Remove(item);
                }

                if (basketItems.Count > 0)
                {
                    string updatedCookie = JsonConvert.SerializeObject(basketItems);
                    Response.Cookies.Append("basket", updatedCookie, new Microsoft.AspNetCore.Http.CookieOptions
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

        [HttpPost]
        [Authorize(Roles = "Member")]
        public async Task<IActionResult> CreateCheckoutSession([FromBody] OrderVM orderVM)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var basketItems = await _context.BasketItems
                .Where(bi => bi.AppUserId == userId)
                .Include(bi => bi.Product)
                .ToListAsync();

            if (!basketItems.Any())
                return BadRequest(new { error = "Your basket is empty." });

            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string> { "card" },
                LineItems = basketItems.Select(bi => new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        UnitAmount = (long)(bi.Product.Price * 100),
                        Currency = "usd",
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = bi.Product.Name,
                        },
                    },
                    Quantity = bi.Count,
                }).ToList(),
                Mode = "payment",
                SuccessUrl = Url.Action("OrderConfirmation", "Basket", null, Request.Scheme) + "?session_id={CHECKOUT_SESSION_ID}",
                CancelUrl = Url.Action("CheckOut", "Basket", null, Request.Scheme),
                Metadata = new Dictionary<string, string>
        {
            { "ZipCode", orderVM.ZipCode ?? "" },
            { "PhoneNumber", orderVM.PhoneNumber ?? "" }
        }
            };

            var service = new SessionService();
            Session session = service.Create(options);

            return Json(new { sessionId = session.Id });
        }


        [Authorize(Roles = "Member")]
        public async Task<IActionResult> CheckOut()
        {
            var zipCodes = await _context.DeliverableAddress.Select(d => d.PinCode).Distinct().ToListAsync();
            ViewBag.ZipCodes = zipCodes;

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var basketItems = await _context.BasketItems
                .Where(bi => bi.AppUserId == userId)
                .Include(bi => bi.Product)
                .ToListAsync();

            var vm = new OrderVM
            {
                BasketInOrderVMs = basketItems.Select(bi => new BasketInOrderVM
                {
                    Count = bi.Count,
                    Name = bi.Product.Name,
                    Price = bi.Product.Price,
                    SubTotal = bi.Product.Price * bi.Count,
                }).ToList()
            };

            return View(vm);
        }

        [HttpPost]
        [Authorize(Roles = "Member")]
        public async Task<IActionResult> CheckOut(OrderVM orderVM)
        {
            var zipCodes = await _context.DeliverableAddress.Select(d => d.PinCode).Distinct().ToListAsync();
            ViewBag.ZipCodes = zipCodes;

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var basketItems = await _context.BasketItems
                .Where(bi => bi.AppUserId == userId)
                .Include(bi => bi.Product)
                .ToListAsync();

            if (!basketItems.Any())
            {
                ModelState.AddModelError("", "Your basket is empty.");
                return View(orderVM);
            }

            var enteredZip = orderVM.ZipCode?.Trim();

            bool isDeliverable = await _context.DeliverableAddress
                .AnyAsync(d => d.PinCode.Trim() == enteredZip);

            if (!isDeliverable)
            {
                ModelState.AddModelError("ZipCode", "Sorry, we do not deliver to this Zip Code.");
                orderVM.BasketInOrderVMs = basketItems.Select(bi => new BasketInOrderVM
                {
                    Count = bi.Count,
                    Name = bi.Product.Name,
                    Price = bi.Product.Price,
                    SubTotal = bi.Product.Price * bi.Count,
                }).ToList();
                return View(orderVM);
            }

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

            var lineItems = basketItems.Select(bi => new SessionLineItemOptions
            {
                PriceData = new SessionLineItemPriceDataOptions
                {
                    UnitAmount = (long)(bi.Product.Price * 100),
                    Currency = "usd",
                    ProductData = new SessionLineItemPriceDataProductDataOptions
                    {
                        Name = bi.Product.Name,
                    },
                },
                Quantity = bi.Count,
            }).ToList();

            var domain = $"{Request.Scheme}://{Request.Host}";

            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string> { "card" },
                LineItems = lineItems,
                Mode = "payment",
                SuccessUrl = Url.Action(nameof(OrderConfirmation), "Basket", null, Request.Scheme) + "?session_id={CHECKOUT_SESSION_ID}",
                CancelUrl = Url.Action(nameof(CheckOut), "Basket", null, Request.Scheme),
                Metadata = new Dictionary<string, string>
        {
            { "UserId", userId },
            { "ZipCode", enteredZip },
            { "PhoneNumber", orderVM.PhoneNumber ?? "" }
        }
            };

            var service = new SessionService();
            Session session = service.Create(options);

            return Redirect(session.Url);
        }



        public async Task<IActionResult> OrderConfirmation(string session_id)
        {
            var service = new SessionService();
            var session = await service.GetAsync(session_id);

            if (session == null || session.PaymentStatus != "paid")
                return RedirectToAction("CheckOut");

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var basketItems = await _context.BasketItems
                .Where(bi => bi.AppUserId == userId)
                .Include(bi => bi.Product)
                .ToListAsync();

            var order = new AppOrder
            {
                Address = session.Metadata.ContainsKey("ZipCode") ? session.Metadata["ZipCode"] : "Unknown",
                PhoneNumber = session.Metadata.ContainsKey("PhoneNumber") ? session.Metadata["PhoneNumber"] : "Unknown",
                Status = OrderStatus.Completed,
                CreatedAt = DateTime.UtcNow,
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

            return View(order);
        }


    }

}
