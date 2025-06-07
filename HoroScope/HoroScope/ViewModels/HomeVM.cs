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
        public List<New>? News { get; set; }
    }
}
