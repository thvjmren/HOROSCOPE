namespace HoroScope.Models
{
    public class PlanetZodiac
    {
        public int Id { get; set; }
        public int PlanetId { get; set; }
        public Planet Planet { get; set; }

        public int ZodiacId { get; set; }
        public Zodiac Zodiac { get; set; }
    }
}
