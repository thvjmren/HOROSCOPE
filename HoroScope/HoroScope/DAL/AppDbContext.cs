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
        public DbSet<AboutUs> AboutUs { get; set; }
        public DbSet<Setting> Settings { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<ProductCategory> ProductCategories { get; set; }
        public DbSet<Zodiac> Zodiacs { get; set; }
        public DbSet<ZodiacElement> ZodiacElements { get; set; }
        public DbSet<New> News { get; set; }
        public DbSet<Partner> Partners { get; set; }
        public DbSet<Expert> Experts { get; set; }
    }
}
