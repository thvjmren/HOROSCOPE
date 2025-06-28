using HoroScope.Models.Base;

namespace HoroScope.Models
{
    public class ProductReview : BaseEntity
    {
        public string ReviewerName { get; set; }
        public string? Comment { get; set; }
        public int Rating { get; set; }
        public int ProductId { get; set; }
        public Product Product { get; set; }
    }

}
