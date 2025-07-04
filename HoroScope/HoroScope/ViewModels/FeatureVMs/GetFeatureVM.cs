using HoroScope.Models;

namespace HoroScope.ViewModels
{
    public class GetFeatureVM
    {
        public string Name { get; set; }
        public int? ProductCategoryId { get; set; }
        public ProductCategory? ProductCategory { get; set; }
        public List<FeatureValue>? FeatureValues { get; set; }
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsDeleted { get; set; }
    }
}
