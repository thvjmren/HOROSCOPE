using HoroScope.DAL;
using HoroScope.Interfaces;
using HoroScope.Models;
using HoroScope.ViewModels;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace HoroScope.Services.Implementations
{
    public class LayoutService : ILayoutService
    {
        private readonly AppDbContext _context;
        private readonly IHttpContextAccessor _http;

        public LayoutService(AppDbContext context, IHttpContextAccessor http)
        {
            _context = context;
            _http = http;
        }

        public async Task<List<BasketItemVM>> GetBasketAsync()
        {
            List<BasketCookieItemVM> cookieItemVM;
            string cookie = _http.HttpContext.Request.Cookies["basket"];

            List<BasketItemVM> basketItemVMs = new();

            if (cookie is null) { return basketItemVMs; }

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
            return basketItemVMs;
        }

        public async Task<Dictionary<string, string>> GetSettingsAsync()
        {
            Dictionary<string, string> settings = await _context.Settings.ToDictionaryAsync(s => s.Key, s => s.Value);
            return settings;
        }

    }
}
