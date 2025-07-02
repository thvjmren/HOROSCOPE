using HoroScope.Models.Base;
using HoroScope.Utilities.Enums;
using System.ComponentModel.DataAnnotations;

namespace HoroScope.Models
{
    public class Order : BaseEntity
    {
        public string Address { get; set; }
        [MaxLength(20)]
        public string PhoneNumber { get; set; }
        public string UserId { get; set; }
        public AppUser? User { get; set; }

        public OrderStatus Status { get; set; }
        public ICollection<OrderItem>? OrderItems { get; set; }
    }
}
