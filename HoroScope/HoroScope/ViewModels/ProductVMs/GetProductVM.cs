namespace HoroScope.ViewModels
{
    public class GetProductVM
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public decimal Discount { get; set; }
        public string? ZodiacName { get; set; }

        public string CategoryName { get; set; }

        public string? MainImage { get; set; }

        public int Stock { get; set; }
        public int SalesCount { get; set; }
        public int ViewsCount { get; set; }

        public double Rating { get; set; }
        public int ReviewCount { get; set; }

        public bool FreeShipping { get; set; }
        public bool CodAvailable { get; set; }

        public int ShippingDays { get; set; }
        public DateTime CreatedAt { get; set; }
    }

}
