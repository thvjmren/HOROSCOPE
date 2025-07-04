using HoroScope.DAL;
using HoroScope.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HoroScope.ViewComponents
{
    public class SubscriptionViewComponent : ViewComponent
    {
        private readonly AppDbContext _context;

        public SubscriptionViewComponent(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync(int? categoryId = null)
        {
            var subscriptionPlansQuery = _context.SubscriptionPlans.Where(s => !s.IsDeleted);

            var subscriptionPlans = await subscriptionPlansQuery.ToListAsync();

            var model = new HomeVM
            {
                SubscriptionPlans = subscriptionPlans
            };

            return View(model);
        }
    }
}
