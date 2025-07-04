using HoroScope.Models;

namespace HoroScope.ViewModels
{
    public class HomeVM
    {
        public List<Service>? Services { get; set; }
        public List<ServiceCategory>? ServiceCategories { get; set; }
        public List<AboutUs>? AboutUs { get; set; }
        public List<Product>? Products { get; set; }
        public List<ProductCategory>? ProductCategories { get; set; }
        public List<Zodiac>? Zodiacs { get; set; }
        public List<ZodiacElement>? ZodiacElements { get; set; }
        public List<Blog>? Blogs { get; set; }
        public List<Partner>? Partners { get; set; }
        public List<Expert>? Experts { get; set; }
        public List<SubscriptionPlan>? SubscriptionPlans { get; set; }
    }
}
