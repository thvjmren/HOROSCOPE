using HoroScope.Models.Base;

namespace HoroScope.Models
{
    public class Blog : BaseEntity
    {
        public string Title { get; set; }
        public string Content { get; set; }
        public string Image { get; set; }
        public int BlogCategoryId { get; set; }
        public BlogCategory? BlogCategory { get; set; }
        public List<BlogImages>? BlogImages { get; set; }
    }
}
