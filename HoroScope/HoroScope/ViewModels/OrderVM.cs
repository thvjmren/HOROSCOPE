using HoroScope.Utilities.Enums;

namespace HoroScope.ViewModels
{
    public class OrderVM
    {
        public int Id { get; set; }
        public OrderStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public int TotalItems { get; set; }
        public decimal TotalPrice { get; set; }
    }
}
