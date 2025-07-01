using HoroScope.ViewModels;

namespace HoroScope.Interfaces
{
    public interface ILayoutService
    {
        Task<Dictionary<string, string>> GetSettingsAsync();
        Task<List<BasketItemVM>> GetBasketAsync();
    }
}
