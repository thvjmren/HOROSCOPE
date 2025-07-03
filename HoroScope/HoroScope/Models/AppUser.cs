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
        public ICollection<BasketItem> BasketItems { get; set; }
        public ICollection<WishlistItem> WishlistItems { get; set; }
        public ICollection<UserSubscription> UserSubscriptions { get; set; }
        public DateTime? BirthDate { get; set; }
        public string? BirthTime { get; set; }
        public string? BirthPlace { get; set; }
        public string? ProfileImageUrl { get; set; }
        public string? SunSign { get; set; }
    }

}
