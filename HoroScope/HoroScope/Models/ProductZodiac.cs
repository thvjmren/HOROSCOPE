namespace HoroScope.Models
{
    public class ProductZodiac
    {
        public int ProductId { get; set; }
        public Product Product { get; set; }

        public int ZodiacId { get; set; }
        public Zodiac Zodiac { get; set; }
    }
}
