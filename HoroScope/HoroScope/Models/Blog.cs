using HoroScope.Models.Base;
using System.ComponentModel.DataAnnotations.Schema;

namespace HoroScope.Models
{
    public class Blog : BaseEntity
    {
        public string Title { get; set; }
        public string Content { get; set; }
        public string Image { get; set; }
        public int BlogCategoryId { get; set; }
        public BlogCategory? BlogCategory { get; set; }
        [NotMapped]
        public string UserName { get; set; }

        [NotMapped]
        public int LikesCount { get; set; }

        public ICollection<BlogComment>? Comments { get; set; }
        public ICollection<BlogLike>? Likes { get; set; }
        public int CommentsCount { get; set; }

    }
}
