using HoroScope.Models.Base;

namespace HoroScope.Models
{
    public class BlogLike : BaseEntity
    {
        public int BlogId { get; set; }
        public Blog Blog { get; set; }

        public string AppUserId { get; set; }
        public AppUser AppUser { get; set; }

    }

}
