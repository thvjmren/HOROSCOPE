using HoroScope.Models.Base;

namespace HoroScope.Models
{
    public class BlogCategory : BaseEntity
    {
        public string Name { get; set; }
        public List<Blog>? Blogs { get; set; }
    }
}
