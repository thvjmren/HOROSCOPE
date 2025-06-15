using HoroScope.Models;

namespace HoroScope.ViewModels
{
    public class BlogVM
    {
        public List<Blog>? Blogs { get; set; }
        public List<BlogCategory>? BlogCategories { get; set; }
        public List<Blog>? RecentNews { get; set; }
        public int BlogCount { get; set; }
    }
}
