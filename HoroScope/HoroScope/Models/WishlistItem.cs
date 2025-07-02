namespace HoroScope.Models
{
    public class WishlistItem
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public Product Product { get; set; }

        public string AppUserId { get; set; }
        public AppUser AppUser { get; set; }
    }
}
