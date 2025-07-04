using HoroScope.Models;
using System.ComponentModel.DataAnnotations;

namespace HoroScope.ViewModels
{
    public class UpdateBlogVM
    {
        public int Id { get; set; }

        [Required]
        public string Title { get; set; }

        [Required]

        public string Content { get; set; }

        [Required]
        public int? CategoryId { get; set; }
        public List<BlogCategory>? Categories { get; set; }

        public string Image { get; set; }
        public IFormFile? Photo { get; set; }

    }
}
