using HoroScope.Models;
using System.ComponentModel.DataAnnotations;

namespace HoroScope.ViewModels
{
    public class UpdateProductVM
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public decimal? Price { get; set; }

        public string Description { get; set; }

        [Required]
        public int? CategoryId { get; set; }

        public string PrimaryImage { get; set; }
        public IFormFile? MainPhoto { get; set; }

        public List<ProductCategory>? Categories { get; set; }

        public List<int> SelectedZodiacIds { get; set; } = new List<int>();

        public List<Zodiac>? Zodiacs { get; set; }

        public List<FeatureSelectionVM>? FeatureSelections { get; set; }
    }
}
