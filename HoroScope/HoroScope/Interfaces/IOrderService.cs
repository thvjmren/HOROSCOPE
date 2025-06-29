using HoroScope.Utilities.Enums;

namespace HoroScope.Interfaces
{
    public interface IOrderService
    {
        Task<bool> UpdateOrderStatusAsync(int orderId, OrderStatus newStatus, CancellationToken cancellationToken = default);
    }
}
