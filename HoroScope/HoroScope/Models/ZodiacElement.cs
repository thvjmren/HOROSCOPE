using HoroScope.Models.Base;

namespace HoroScope.Models
{
    public class ZodiacElement : BaseEntity
    {
        public string Name { get; set; }
        public List<Zodiac>? Zodiacs { get; set; }
    }
}
