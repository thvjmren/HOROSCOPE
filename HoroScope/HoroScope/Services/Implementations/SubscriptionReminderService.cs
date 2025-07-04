using HoroScope.DAL;
using HoroScope.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HoroScope.Services.Implementations
{
    public class SubscriptionReminderService : ISubscriptionReminderService
    {
        private readonly AppDbContext _context;
        private readonly IEmailService _emailService;

        public SubscriptionReminderService(AppDbContext context, IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        public async Task SendExpiringRemindersAsync()
        {
            var targetDate = DateTime.UtcNow.Date.AddDays(3);

            var expiringSubs = await _context.UserSubscriptions
                .Include(s => s.AppUser)
                .Include(s => s.SubscriptionPlan)
                .Where(s => s.IsActive && s.EndDate.Date == targetDate)
                .ToListAsync();

            foreach (var sub in expiringSubs)
            {
                string subject = "⏳ Your Subscription Will Expire Soon";
                string message = $"Hello {sub.AppUser.UserName},<br/><br/>" +
                                 $"Your <b>{sub.SubscriptionPlan.Name}</b> plan will expire in 3 days on <b>{sub.EndDate.ToShortDateString()}</b>.<br/>" +
                                 $"Don't forget to renew it to continue enjoying the service.";

                await _emailService.SendMailAsync(sub.AppUser.Email, subject, message);
            }
        }
    }
}
