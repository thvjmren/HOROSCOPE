namespace HoroScope.ViewModels
{
    public class GetBlogVM
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public string Image { get; set; }
        public string BlogCategoryName { get; set; }
        public string AuthorName { get; set; }
        public int LikesCount { get; set; }
        public int CommentsCount { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
