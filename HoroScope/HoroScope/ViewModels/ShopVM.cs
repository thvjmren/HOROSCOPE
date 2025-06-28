using HoroScope.Models;

namespace HoroScope.ViewModels
{
    public class ShopVM
    {
        public List<Product>? NewProducts { get; set; }
        public List<ProductCategory>? ProductCategories { get; set; }
        public List<ProductCategory>? TopCategories { get; set; }
        public Product Product { get; set; }
        public int ProductCount { get; set; }
        public string SelectedSort { get; set; }
        public int? SelectedCategoryId { get; set; }
        public PaginatedVM<Product>? Products { get; set; }
        public int PageSize { get; set; }
        public string Search { get; set; }
        public int Key { get; set; }
    }
}
