using HoroScope.DAL;
using HoroScope.ViewModels;
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
        public async Task<IViewComponentResult> InvokeAsync(int? categoryId = null)
        {
            var categories = await _context.ServiceCategories.ToListAsync();

            var servicesQuery = _context.Services.Where(s => !s.IsDeleted);
            if (categoryId != null)
                servicesQuery = servicesQuery.Where(s => s.ServiceCategoryId == categoryId);

            var services = await servicesQuery.ToListAsync();

            var model = new HomeVM
            {
                ServiceCategories = categories,
                Services = services
            };

            return View(model);
        }
    }
}
