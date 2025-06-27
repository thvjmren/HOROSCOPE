using Microsoft.Build.Framework;

namespace HoroScope.ViewModels
{
    public class AddCommentVM
    {
        public int BlogId { get; set; }

        [Required]
        public string Text { get; set; }
    }

}
