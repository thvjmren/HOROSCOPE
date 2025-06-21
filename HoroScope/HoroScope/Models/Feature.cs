using HoroScope.Models.Base;

namespace HoroScope.Models
{
    public class Feature : BaseEntity
    {
        public string Name { get; set; }
        public int? ProductCategoryId { get; set; }
        public ProductCategory? ProductCategory { get; set; }
        public List<FeatureValue>? FeatureValues { get; set; }
    }
}
