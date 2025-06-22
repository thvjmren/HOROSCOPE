using HoroScope.Models;

namespace HoroScope.ViewModels
{
    public class ShopDetailsVM
    {
        public Product Product { get; set; }
        public List<Product>? Products { get; set; }
        public List<Product>? PopularProducts { get; set; }
        public List<ProductReview>? Reviews { get; set; }
        public ProductReviewVM Review { get; set; } = new();
    }
}
