using HoroScope.Models.Base;

namespace HoroScope.Models
{
    public class ServiceCategory : BaseEntity
    {
        public string Name { get; set; }
        public List<Service>? Services { get; set; }
    }
}
