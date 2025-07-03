using HoroScope.Models.Base;

namespace HoroScope.Models
{
    public class SubscriptionPlan : BaseEntity
    {
        public string Name { get; set; }
        public decimal Price { get; set; }
        public int DurationInDays { get; set; }
        public string Description { get; set; }
    }

}
