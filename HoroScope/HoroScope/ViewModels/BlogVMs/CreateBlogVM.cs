using HoroScope.Models;

namespace HoroScope.ViewModels
{
    public class CreateBlogVM
    {
        public string Title { get; set; }
        public string Content { get; set; }
        public IFormFile Image { get; set; }
        public int BlogCategoryId { get; set; }
        public List<BlogCategory>? BlogCategories { get; set; }
    }
}
