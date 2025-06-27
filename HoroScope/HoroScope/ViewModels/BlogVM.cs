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
        public List<ArchiveVM>? Archives { get; set; }
        public int? SelectedYear { get; set; }
        public int? SelectedMonth { get; set; }
        public int BlogLikesCount { get; set; }
        public int BlogCommentsCount { get; set; }
        public bool UserHasLiked { get; set; }
        public List<BlogComment>? BlogComments { get; set; }

    }
}
