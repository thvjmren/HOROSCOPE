using HoroScope.Models.Base;
using HoroScope.Utilities.Enums;

namespace HoroScope.Models
{
    public class Order : BaseEntity

    {
        public string UserId { get; set; }
        public DateTime OrderDate { get; set; }
        public OrderStatus Status { get; set; }
        public AppUser? User { get; set; }
        public ICollection<OrderItem>? OrderItems { get; set; }
    }
}
