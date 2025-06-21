using HoroScope.Models.Base;

namespace HoroScope.Models
{
    public class FeatureValue : BaseEntity
    {
        public string Value { get; set; }

        public int FeatureId { get; set; }
        public Feature Feature { get; set; }

        public List<ProductFeatureValue>? ProductFeatureValues { get; set; }
    }
}
