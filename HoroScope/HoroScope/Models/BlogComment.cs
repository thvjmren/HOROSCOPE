
using HoroScope.Models.Base;
using System.ComponentModel.DataAnnotations;

namespace HoroScope.Models
{
    public class BlogComment : BaseEntity
    {
        [Required]
        public string Text { get; set; }

        [Required]
        public int BlogId { get; set; }
        public Blog? Blog { get; set; }


        //[Required]
        public string? AppUserId { get; set; }
        public AppUser? AppUser { get; set; }

        //[Required]
        [EmailAddress]
        public string? Email { get; set; }
    }
}
