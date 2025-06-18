using HoroScope.Models;

namespace HoroScope.ViewModels
{
    public class BlogDetailsVM
    {
        public Blog Blog { get; set; }
        public List<Blog>? Blogs { get; set; }
        public List<BlogCategory>? BlogCategories { get; set; }
        public List<Blog>? RecentNews { get; set; }
    }
}
