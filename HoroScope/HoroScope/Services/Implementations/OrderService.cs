using HoroScope.DAL;
using HoroScope.Interfaces;
using HoroScope.Utilities.Enums;
using HoroScope.ViewModels;
using Microsoft.EntityFrameworkCore;


namespace HoroScope.Implementations
{
    public class OrderService : IOrderService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<OrderService> _logger;

        public OrderService(AppDbContext context, ILogger<OrderService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<OrderVM>> GetAllOrderVMsAsync(string userId, bool isAdmin)
        {
            var query = _context.Orders.Include(o => o.OrderItems).AsQueryable();

            if (!isAdmin)
            {
                query = query.Where(o => o.UserId == userId);
            }

            return await query.Select(o => new OrderVM
            {
                Id = o.Id,
                Status = o.Status,
                CreatedAt = o.CreatedAt,
                TotalItems = o.OrderItems.Sum(i => i.Quantity),
                TotalPrice = o.OrderItems.Sum(i => i.Quantity * i.Product.Price)
            }).ToListAsync();
        }



        private bool IsValidStatusTransition(OrderStatus currentStatus, OrderStatus newStatus)
        {
            return currentStatus switch
            {
                OrderStatus.Pending => newStatus == OrderStatus.Processing || newStatus == OrderStatus.Cancelled,
                OrderStatus.Processing => newStatus == OrderStatus.Completed || newStatus == OrderStatus.Cancelled,
                OrderStatus.Completed => false,
                OrderStatus.Cancelled => false,
                _ => false,
            };
        }

        public async Task<bool> UpdateOrderStatusAsync(int orderId, OrderStatus newStatus, CancellationToken cancellationToken = default)
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.Id == orderId, cancellationToken);

            if (order == null)
            {
                _logger.LogWarning("Order with Id {OrderId} not found.", orderId);
                return false;
            }

            if (order.Status == newStatus)
            {
                _logger.LogInformation("Order {OrderId} status is already {Status}.", orderId, newStatus);
                return true;
            }

            if (!IsValidStatusTransition(order.Status, newStatus))
            {
                _logger.LogWarning("Invalid status transition from {CurrentStatus} to {NewStatus} for order {OrderId}.", order.Status, newStatus, orderId);
                return false;
            }

            using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                order.Status = newStatus;

                if (newStatus == OrderStatus.Completed)
                {
                    var productIds = order.OrderItems.Select(i => i.ProductId).Distinct().ToList();

                    var products = await _context.Products
                        .Where(p => productIds.Contains(p.Id))
                        .ToDictionaryAsync(p => p.Id, cancellationToken);

                    foreach (var item in order.OrderItems)
                    {
                        if (products.TryGetValue(item.ProductId, out var product))
                        {
                            product.SalesCount += item.Quantity;
                        }
                        else
                        {
                            _logger.LogWarning("Product {ProductId} not found when updating sales count for order {OrderId}.", item.ProductId, order.Id);
                        }
                    }
                }
                else if (newStatus == OrderStatus.Cancelled)
                {
                    var productIds = order.OrderItems.Select(i => i.ProductId).Distinct().ToList();

                    var products = await _context.Products
                        .Where(p => productIds.Contains(p.Id))
                        .ToDictionaryAsync(p => p.Id, cancellationToken);

                    foreach (var item in order.OrderItems)
                    {
                        if (products.TryGetValue(item.ProductId, out var product))
                        {
                            product.Stock += item.Quantity;
                        }
                        else
                        {
                            _logger.LogWarning("Product {ProductId} not found when restoring stock for cancelled order {OrderId}.", item.ProductId, order.Id);
                        }
                    }

                    _logger.LogInformation("Stock restored for cancelled order {OrderId}.", order.Id);
                }


                await _context.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                _logger.LogInformation("Order {OrderId} status changed to {Status} successfully.", orderId, newStatus);

                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                _logger.LogError(ex, "Error updating order status for order {OrderId}", orderId);
                throw;
            }
        }

    }
}
