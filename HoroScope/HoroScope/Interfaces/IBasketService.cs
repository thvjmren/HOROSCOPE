using HoroScope.ViewModels;

namespace HoroScope.Interfaces
{
    public interface IBasketService
    {
        Task<List<BasketItemVM>> GetBasketAsync();
    }
}
