using HoroScope.Models;

namespace HoroScope.ViewModels
{
    public class AboutUsVM
    {
        public List<Service>? Services { get; set; }
        public List<ServiceCategory>? ServiceCategories { get; set; }
        public List<AboutUs>? AboutUs { get; set; }
        public List<Partner>? Partners { get; set; }
        public List<Expert>? Experts { get; set; }
        public string? ContactNumber { get; set; }
    }
}
