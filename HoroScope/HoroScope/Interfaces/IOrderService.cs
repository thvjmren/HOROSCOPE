using HoroScope.Utilities.Enums;
using HoroScope.ViewModels;

namespace HoroScope.Interfaces
{
    public interface IOrderService
    {
        Task<List<OrderVM>> GetAllOrderVMsAsync(string userId, bool isAdmin);
        Task<bool> UpdateOrderStatusAsync(int orderId, OrderStatus newStatus, CancellationToken cancellationToken = default);
    }
}
