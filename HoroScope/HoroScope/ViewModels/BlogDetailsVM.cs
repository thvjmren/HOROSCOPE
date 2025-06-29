using HoroScope.Models;

namespace HoroScope.ViewModels
{
    public class BlogDetailsVM
    {
        public Blog? Blog { get; set; }
        public List<Blog>? Blogs { get; set; }
        public List<BlogComment>? BlogComments { get; set; }
        public BlogComment? BlogComment { get; set; }
        public List<BlogCategory>? BlogCategories { get; set; }
        public List<Blog>? RecentNews { get; set; }
        public List<ArchiveVM> Archives { get; set; }
        public int BlogLikesCount { get; set; }
        public bool UserHasLiked { get; set; }
        public BlogCommentVM BlogCommentVM { get; set; }
        public string Search { get; set; }
        public int? SelectedCategoryId { get; set; }
    }
}
