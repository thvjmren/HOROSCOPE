using HoroScope.DAL;
using HoroScope.Models;
using HoroScope.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace HoroScope.Controllers
{
    public class BasketController : Controller
    {
        private readonly AppDbContext _context;

        public BasketController(AppDbContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index()
        {
            List<BasketCookieItemVM> cookieItemVM;
            string cookie = Request.Cookies["basket"];

            List<BasketItemVM> basketItemVMs = new();

            if (cookie is null) { return View(basketItemVMs); }

            cookieItemVM = JsonConvert.DeserializeObject<List<BasketCookieItemVM>>(cookie);

            foreach (var item in cookieItemVM)
            {
                Product product = await _context.Products
                    .Include(p => p.ProductImages.Where(pi => pi.IsPrimary == true))
                    .FirstOrDefaultAsync(p => p.Id == item.Id);
                if (product != null)
                {
                    basketItemVMs.Add(new BasketItemVM
                    {
                        Id = product.Id,
                        Name = product.Name,
                        Image = product.ProductImages[0].Image,
                        Price = product.Price,
                        Count = item.Count,
                        SubTotal = (decimal)(item.Count * product.Price)
                    });
                }
            }
            return View(basketItemVMs);
        }

        public async Task<IActionResult> AddBasket(int? id)
        {
            if (id == null || id <= 0) return BadRequest();

            bool result = await _context.Products.AnyAsync(p => p.Id == id);
            if (!result) return NotFound();

            List<BasketCookieItemVM> basketCookies;

            string cookies = Request.Cookies["basket"];
            if (cookies != null)
            {
                basketCookies = JsonConvert.DeserializeObject<List<BasketCookieItemVM>>(cookies);

                BasketCookieItemVM existed = basketCookies.FirstOrDefault(bc => bc.Id == id);
                if (existed != null)
                {
                    existed.Count++;
                }
                else
                {
                    basketCookies.Add(new()
                    {
                        Id = id.Value,
                        Count = 1
                    });
                }
            }
            else
            {
                basketCookies = new();
                basketCookies.Add(new()
                {
                    Id = id.Value,
                    Count = 1
                });
            }
            string basketJson = JsonConvert.SerializeObject(basketCookies);
            Response.Cookies.Append("basket", basketJson);

            return RedirectToAction(nameof(Index), "Shop");
        }

        public IActionResult GetBasket()
        {
            return Content(Request.Cookies["basket"]);
        }

        public IActionResult RemoveFromBasket(int? id)
        {
            if (id == null || id <= 0) return BadRequest();

            string basketCookie = Request.Cookies["basket"];
            if (string.IsNullOrEmpty(basketCookie)) return RedirectToAction(nameof(Index));

            var basketItems = JsonConvert.DeserializeObject<List<BasketCookieItemVM>>(basketCookie);

            var itemToRemove = basketItems.FirstOrDefault(x => x.Id == id);
            if (itemToRemove != null)
            {
                basketItems.Remove(itemToRemove);

                if (basketItems.Count > 0)
                {
                    string updatedBasket = JsonConvert.SerializeObject(basketItems);
                    Response.Cookies.Append("basket", updatedBasket);
                }
                else
                {
                    Response.Cookies.Delete("basket");
                }
            }

            return RedirectToAction(nameof(Index));
        }

        public IActionResult IncreaseCount(int? id)
        {
            if (id == null || id <= 0) return BadRequest();

            string basketCookie = Request.Cookies["basket"];
            if (string.IsNullOrEmpty(basketCookie)) return RedirectToAction(nameof(Index));

            var basketItems = JsonConvert.DeserializeObject<List<BasketCookieItemVM>>(basketCookie);
            var item = basketItems.FirstOrDefault(x => x.Id == id);

            if (item != null)
            {
                item.Count++;
                string updatedCookie = JsonConvert.SerializeObject(basketItems);
                Response.Cookies.Append("basket", updatedCookie);
            }

            return RedirectToAction(nameof(Index));
        }
        public IActionResult DecreaseCount(int? id)
        {
            if (id == null || id <= 0) return BadRequest();

            string basketCookie = Request.Cookies["basket"];
            if (string.IsNullOrEmpty(basketCookie)) return RedirectToAction(nameof(Index));

            var basketItems = JsonConvert.DeserializeObject<List<BasketCookieItemVM>>(basketCookie);
            var item = basketItems.FirstOrDefault(x => x.Id == id);

            if (item != null)
            {
                if (item.Count > 1)
                {
                    item.Count--;
                }
                else
                {
                    basketItems.Remove(item);
                }

                if (basketItems.Count > 0)
                {
                    string updatedCookie = JsonConvert.SerializeObject(basketItems);
                    Response.Cookies.Append("basket", updatedCookie);
                }
                else
                {
                    Response.Cookies.Delete("basket");
                }
            }

            return RedirectToAction(nameof(Index));
        }


    }
}