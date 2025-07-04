using System.ComponentModel.DataAnnotations;

namespace HoroScope.ViewModels
{
    public class CreateBlogCategoryVM
    {
        [Required(ErrorMessage = "Category name is required.")]
        [StringLength(100, ErrorMessage = "Category name cannot exceed 100 characters.")]
        public string Name { get; set; }

        public DateTime CreatedAt { get; set; }
        public bool IsDeleted { get; set; }
    }
}
