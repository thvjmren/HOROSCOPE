using HoroScope.Models.Base;

namespace HoroScope.Models
{
    public class ProductCategory : BaseEntity
    {
        public string Name { get; set; }
        public List<Product>? Products { get; set; }
        public List<Feature>? Features { get; set; }
    }
}
