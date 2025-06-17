using HoroScope.DAL;
using HoroScope.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HoroScope.ViewComponents
{
    public class ServiceViewComponent : ViewComponent
    {
        private readonly AppDbContext _context;

        public ServiceViewComponent(AppDbContext context)
        {
            _context = context;
        }
        public async Task<IViewComponentResult> InvokeAsync()
        {
            List<Product>? products = await _context.Products.Include(p => p.ProductImages.Where(pi => pi.IsPrimary != null)).ToListAsync();
            return View(products);
        }
    }
}
