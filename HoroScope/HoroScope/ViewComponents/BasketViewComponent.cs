using HoroScope.Interfaces;
using Microsoft.AspNetCore.Mvc;

public class BasketViewComponent : ViewComponent
{
    private readonly IBasketService _basketService;

    public BasketViewComponent(IBasketService basketService)
    {
        _basketService = basketService;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        var basketItems = await _basketService.GetBasketAsync();
        return View(basketItems);
    }
}
