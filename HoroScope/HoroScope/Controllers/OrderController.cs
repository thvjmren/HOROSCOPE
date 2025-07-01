using HoroScope.Interfaces;
using HoroScope.Models;
using HoroScope.Utilities.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

[Authorize]
public class OrderController : Controller
{
    private readonly IOrderService _orderService;
    private readonly UserManager<AppUser> _userManager;

    public OrderController(IOrderService orderService, UserManager<AppUser> userManager)
    {
        _orderService = orderService;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var user = await _userManager.GetUserAsync(User);
        var userId = user.Id;

        var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");

        var orders = await _orderService.GetAllOrderVMsAsync(userId, isAdmin);

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

        return RedirectToAction(nameof(Index));
    }
}
