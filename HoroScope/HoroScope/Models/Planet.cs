using HoroScope.Models.Base;

namespace HoroScope.Models
{
    public class Planet : BaseEntity
    {
        public string Name { get; set; }

        public List<PlanetZodiac>? PlanetZodiacs { get; set; }
    }

}
