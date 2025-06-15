using HoroScope.Models.Base;

namespace HoroScope.Models
{
    public class Contact : BaseEntity
    {
        public string Icon { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string FormTitle { get; set; }
        public string FormDescription { get; set; }
    }
}
