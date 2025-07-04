namespace HoroScope.Interfaces
{
    public interface ISubscriptionReminderService
    {
        Task SendExpiringRemindersAsync();
    }
}
