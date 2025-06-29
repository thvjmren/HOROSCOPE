using System.ComponentModel.DataAnnotations;

namespace HoroScope.ViewModels
{
    public class ProductReviewVM
    {
        [Required]
        public int ProductId { get; set; }

        //[Required(ErrorMessage = "Please enter your comment.")]
        [StringLength(1000, ErrorMessage = "Comment cannot exceed 1000 characters.")]
        public string? Comment { get; set; }

        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5.")]
        public int Rating { get; set; }
    }
}
