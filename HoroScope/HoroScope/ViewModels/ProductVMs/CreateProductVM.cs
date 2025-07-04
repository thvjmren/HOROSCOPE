using HoroScope.Models;
using System.ComponentModel.DataAnnotations;

namespace HoroScope.ViewModels
{
    public class CreateProductVM
    {
        [Required(ErrorMessage = "Product name is required")]
        [StringLength(100, ErrorMessage = "Name max length is 100 characters")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Price is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be positive")]
        public decimal? Price { get; set; }

        [Required(ErrorMessage = "Description is required")]
        [StringLength(2000, ErrorMessage = "Description max length is 2000 characters")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Category is required")]
        public int? CategoryId { get; set; }

        [Required(ErrorMessage = "Main photo is required")]
        public IFormFile MainPhoto { get; set; }

        public List<ProductCategory>? Categories { get; set; }
        public List<FeatureSelectionVM> FeatureSelections { get; set; } = new List<FeatureSelectionVM>();

        public List<int> SelectedZodiacIds { get; set; } = new List<int>();

        public List<Zodiac>? Zodiacs { get; set; }
    }
}
