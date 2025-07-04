using HoroScope.Models;

namespace HoroScope.ViewModels
{
    public class CreateFeatureValueVM
    {
        public string Value { get; set; }
        public int FeatureId { get; set; }
        public List<Feature>? Features { get; set; }
    }
}
