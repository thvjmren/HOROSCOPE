using HoroScope.Models.Base;

namespace HoroScope.Models
{
    public class Product : BaseEntity
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public decimal Discount { get; set; }
        public int ProductCategoryId { get; set; }
        public ProductCategory ProductCategory { get; set; }
        public List<ProductImages>? ProductImages { get; set; }
        public List<ProductFeatureValue> ProductFeatureValues { get; set; }
        public List<ProductReview>? ProductReviews { get; set; } = new List<ProductReview>();

        public int Stock { get; set; }
        public int SalesCount { get; set; } = 0;
        public int ViewsCount { get; set; } = 0;
        public int ReviewCount { get; set; } = 0;
        public double Rating { get; set; }

        public int ShippingDays { get; set; } = 3;
        public bool FreeShipping => Price >= 77;
        public bool CodAvailable { get; set; } = true;
        public ICollection<ProductZodiac> ProductZodiacs { get; set; }

    }
}
