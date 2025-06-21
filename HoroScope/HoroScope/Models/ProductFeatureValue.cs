namespace HoroScope.Models
{
    public class ProductFeatureValue
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public Product Product { get; set; }

        public int FeatureValueId { get; set; }
        public FeatureValue FeatureValue { get; set; }
    }
}
