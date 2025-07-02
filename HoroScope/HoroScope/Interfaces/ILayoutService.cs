namespace HoroScope.Interfaces
{
    public interface ILayoutService
    {
        Task<Dictionary<string, string>> GetSettingsAsync();
    }
}
