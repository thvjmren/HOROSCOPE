using HoroScope.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace HoroScope.DAL
{
    public class AppDbContext : IdentityDbContext<AppUser>
    {

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
        public DbSet<Service> Services { get; set; }
        public DbSet<ServiceCategory> ServiceCategories { get; set; }
        public DbSet<Blog> Blogs { get; set; }
        public DbSet<BlogCategory> BlogCategories { get; set; }
        public DbSet<Contact> Contacts { get; set; }
        public DbSet<AboutUs> AboutUs { get; set; }
        public DbSet<Setting> Settings { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<ProductCategory> ProductCategories { get; set; }
        public DbSet<Zodiac> Zodiacs { get; set; }
        public DbSet<ZodiacElement> ZodiacElements { get; set; }
        public DbSet<New> News { get; set; }
        public DbSet<Partner> Partners { get; set; }
        public DbSet<Expert> Experts { get; set; }
        public DbSet<Planet> Planets { get; set; }
        public DbSet<PlanetZodiac> PlanetZodiacs { get; set; }
        public DbSet<Feature> Features { get; set; }
        public DbSet<FeatureValue> FeatureValues { get; set; }
        public DbSet<ProductFeatureValue> ProductFeatureValues { get; set; }
        public DbSet<ProductReview> ProductReviews { get; set; }
        public DbSet<DeliverableAddress> DeliverableAddress { get; set; }
        public DbSet<BlogComment> BlogComments { get; set; }
        public DbSet<BlogLike> BlogLikes { get; set; }
        public DbSet<BasketItem> BasketItems { get; set; }
        public DbSet<WishlistItem> WishlistItems { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
    }
}
