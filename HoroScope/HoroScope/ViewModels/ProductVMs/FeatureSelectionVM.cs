using HoroScope.Models;

namespace HoroScope.ViewModels
{
    public class FeatureSelectionVM
    {
        public int FeatureId { get; set; }
        public string FeatureName { get; set; }
        public List<FeatureValue> Values { get; set; } = new();
        public List<int> SelectedValueIds { get; set; } = new();
    }
}
