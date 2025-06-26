using HoroScope.Models;

namespace HoroScope.ViewModels
{
    public class BlogVM
    {
        public List<BlogCategory>? BlogCategories { get; set; }
        public List<Blog>? RecentNews { get; set; }
        public int BlogCount { get; set; }
        public PaginatedVM<Blog>? Blogs { get; set; }
        public int PageSize { get; set; }
        public int? SelectedCategoryId { get; set; }
    }
}
