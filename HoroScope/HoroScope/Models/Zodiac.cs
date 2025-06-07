using HoroScope.Models.Base;

namespace HoroScope.Models
{
    public class Zodiac : BaseEntity
    {
        public string Name { get; set; }
        public string Icon { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}
