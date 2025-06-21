using HoroScope.Models.Base;

namespace HoroScope.Models
{
    public class Zodiac : BaseEntity
    {
        public string Name { get; set; }
        public string Icon { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int ZodiacElementId { get; set; }
        public ZodiacElement ZodiacElement { get; set; }
        public List<PlanetZodiac> PlanetZodiacs { get; set; }

    }
}
