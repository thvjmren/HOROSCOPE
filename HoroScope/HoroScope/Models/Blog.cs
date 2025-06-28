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
        public BlogCategory BlogCategory { get; set; }

        public string AppUserId { get; set; }
        public AppUser AppUser { get; set; }

        public ICollection<BlogComment> Comments { get; set; }
        public ICollection<BlogLike> Likes { get; set; }

        [NotMapped]
        public int LikesCount => Likes?.Count ?? 0;

        [NotMapped]
        public int CommentsCount => Comments?.Count ?? 0;
    }

}
