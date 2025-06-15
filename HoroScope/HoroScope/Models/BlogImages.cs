using HoroScope.Models.Base;

namespace HoroScope.Models
{
    public class BlogImages : BaseEntity
    {
        public string Image { get; set; }
        public bool? IsPrimary { get; set; }
        public int BlogId { get; set; }
        public Blog Blog { get; set; }
    }
}
