using HoroScope.Models.Base;

namespace HoroScope.Models
{
    public class ProductImages : BaseEntity
    {
        public string Image { get; set; }
        public bool? IsPrimary { get; set; }
        public int ProductId { get; set; }
        public Product Product { get; set; }
    }
}
