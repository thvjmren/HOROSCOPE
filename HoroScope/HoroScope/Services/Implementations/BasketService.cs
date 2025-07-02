using HoroScope.DAL;
using HoroScope.Interfaces;
using HoroScope.Models;
using HoroScope.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Security.Claims;

namespace HoroScope.Services.Implementations
{
    public class BasketService : IBasketService
    {
        private readonly AppDbContext _context;
        private readonly IHttpContextAccessor _http;
        private readonly UserManager<AppUser> _userManager;
        private readonly ClaimsPrincipal _user;
        public BasketService(AppDbContext context, IHttpContextAccessor http, UserManager<AppUser> userManager)
        {
            _context = context;
            _http = http;
            _userManager = userManager;
            _user = http.HttpContext.User;
        }
        public async Task<List<BasketItemVM>> GetBasketAsync()
        {
            List<BasketItemVM> basketItemVMs = new();
            if (_user.Identity.IsAuthenticated)
            {
                basketItemVMs = await _userManager.Users
                    .Include(u => u.BasketItems)
                    .Where(u => u.Id == _user.FindFirstValue(ClaimTypes.NameIdentifier))
                    .SelectMany(u => u.BasketItems).Select(bi => new BasketItemVM
                    {
                        Id = bi.ProductId,
                        Name = bi.Product.Name,
                        Price = bi.Product.Price,
                        Count = bi.Count,
                        Image = bi.Product.ProductImages.FirstOrDefault(pi => pi.IsPrimary == true).Image,
                        SubTotal = bi.Count * bi.Product.Price,
                    }).ToListAsync();
            }

            else
            {

                List<BasketCookieItemVM> cookieItemVM;
                string cookie = _http.HttpContext.Request.Cookies["basket"];


                if (cookie is null) { return basketItemVMs; }

                cookieItemVM = JsonConvert.DeserializeObject<List<BasketCookieItemVM>>(cookie);


                basketItemVMs = await _context.Products.Where(p => cookieItemVM.Select(c => c.Id).Contains(p.Id))
                    .Select(p => new BasketItemVM
                    {
                        Id = p.Id,
                        Name = p.Name,
                        Image = p.ProductImages[0].Image,
                        Price = p.Price,
                    })
                    .ToListAsync();

                basketItemVMs.ForEach(bi =>
                {
                    bi.Count = cookieItemVM.FirstOrDefault(ci => ci.Id == bi.Id).Count;
                    bi.SubTotal = bi.Price * bi.Count;
                });

            }

            return basketItemVMs;
        }
    }
}
