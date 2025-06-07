using HoroScope.Models.Base;

namespace HoroScope.Models
{
    public class AboutUs : BaseEntity
    {
        public string Title { get; set; }
        public string Image { get; set; }
        public string Description { get; set; }
    }
}
