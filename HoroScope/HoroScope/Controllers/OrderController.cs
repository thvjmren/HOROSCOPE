using HoroScope.Interfaces;
using HoroScope.Utilities.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

public class OrderController : Controller
{
    private readonly IOrderService _orderService;

    public OrderController(IOrderService orderService)
    {
        _orderService = orderService;
    }
    public async Task<IActionResult> Index()
    {
        var orders = await _context.Orders
                                   .Include(o => o.OrderItems)
                                   .ToListAsync();

        return View(orders);
    }


    [HttpPost]
    public async Task<IActionResult> UpdateOrderStatus(int orderId, OrderStatus newStatus)
    {
        var result = await _orderService.UpdateOrderStatusAsync(orderId, newStatus);

        if (!result)
        {
            return NotFound();
        }

        return RedirectToAction("Details", new { id = orderId });
    }
}
