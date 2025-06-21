using HoroScope.Models;

namespace HoroScope.ViewModels
{
    public class ShopDetailsVM
    {
        public Product Product { get; set; }
        public List<Product>? Products { get; set; }
        public List<ProductReview>? Reviews { get; set; }
    }
}
