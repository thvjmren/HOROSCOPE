using HoroScope.Models.Base;

namespace HoroScope.Models
{
    public class Service : BaseEntity
    {
        public string Icon { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int ServiceCategoryId { get; set; }
        public ServiceCategory ServiceCategory { get; set; }
    }
}
