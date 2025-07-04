using System.ComponentModel.DataAnnotations;

namespace HoroScope.ViewModels
{
    public class UpdateBlogCategoryVM
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Category name is required.")]
        [StringLength(100, ErrorMessage = "Category name cannot exceed 100 characters.")]
        public string Name { get; set; }

        public bool IsDeleted { get; set; }

    }
}
