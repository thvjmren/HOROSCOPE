using Microsoft.AspNetCore.Identity;

namespace HoroScope.Models
{
    public class AppUser : IdentityUser
    {
        public string Name { get; set; }
        public string Surname { get; set; }
        public string? ImageUrl { get; set; }
        public ICollection<BlogComment>? Comments { get; set; }
        public ICollection<BlogLike>? Likes { get; set; }
        public ICollection<Blog>? Blogs { get; set; }
        public ICollection<Order>? Orders { get; set; }
    }

}
