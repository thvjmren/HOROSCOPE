using HoroScope.DAL;
using HoroScope.Interfaces;
using HoroScope.Models;
using HoroScope.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Stripe.Checkout;

namespace YourNamespace.Controllers
{
    [Authorize]
    public class SubscriptionController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly IEmailService _emailService;

        public SubscriptionController(AppDbContext context, UserManager<AppUser> userManager, IEmailService emailService)
        {
            _context = context;
            _userManager = userManager;
            _emailService = emailService;
        }

        public IActionResult Index()
        {
            var plans = _context.SubscriptionPlans.ToList();
            return View(plans);
        }

        [HttpPost]
        public async Task<IActionResult> Subscribe(int planId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Challenge();

            var plan = await _context.SubscriptionPlans.FindAsync(planId);
            if (plan == null)
                return NotFound();

            var domain = $"{Request.Scheme}://{Request.Host}";

            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string> { "card" },
                LineItems = new List<SessionLineItemOptions>
        {
            new SessionLineItemOptions
            {
                PriceData = new SessionLineItemPriceDataOptions
                {
                    UnitAmount = (long)(plan.Price * 100),
                    Currency = "usd",
                    ProductData = new SessionLineItemPriceDataProductDataOptions
                    {
                        Name = plan.Name,
                        Description = plan.Description
                    }
                },
                Quantity = 1,
            }
        },
                Mode = "payment",
                SuccessUrl = domain + "/Subscription/Success?planId=" + plan.Id + "&session_id={CHECKOUT_SESSION_ID}",
                CancelUrl = domain + "/Subscription/Cancel",
                Metadata = new Dictionary<string, string>
        {
            { "UserId", user.Id },
            { "PlanId", plan.Id.ToString() }
        }
            };

            var service = new SessionService();
            var session = service.Create(options);

            return Redirect(session.Url);
        }

        public async Task<IActionResult> Success(int planId, string session_id)
        {
            var service = new SessionService();
            var session = await service.GetAsync(session_id);

            if (session == null || session.PaymentStatus != "paid")
                return RedirectToAction("Cancel");

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Challenge();

            var activeSubs = _context.UserSubscriptions
                .Where(s => s.AppUserId == user.Id && s.IsActive)
                .ToList();

            foreach (var sub in activeSubs)
            {
                sub.IsActive = false;
                _context.UserSubscriptions.Update(sub);
            }

            var plan = await _context.SubscriptionPlans.FindAsync(planId);
            if (plan == null)
                return NotFound();

            var newSub = new UserSubscription
            {
                AppUserId = user.Id,
                SubscriptionPlanId = plan.Id,
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddDays(plan.DurationInDays),
                IsActive = true,
            };

            _context.UserSubscriptions.Add(newSub);
            await _context.SaveChangesAsync();


            string subject = "Subscription Activated";
            string message = $@"
            <p>Hello <strong>{user.UserName}</strong>,</p>
            <p>Thank you for subscribing to our <strong>{plan.Name}</strong> plan.</p>
            <p>Your subscription is now active until <strong>{newSub.EndDate.ToString("dd.MM.yyyy")}</strong>.</p>
            <p>Enjoy the service!</p>
            <br/>
            <p>Best regards,<br/>HoroScope Team</p>";


            await _emailService.SendMailAsync(user.Email, subject, message);

            return View("Success");
        }

        public IActionResult Cancel()
        {
            return View("Cancel");
        }
        public async Task<IActionResult> MyPlans()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Challenge();

            var subscriptions = _context.UserSubscriptions
                .Where(s => s.AppUserId == user.Id)
                .OrderByDescending(s => s.StartDate)
                .Select(s => new UserSubscriptionVM
                {
                    PlanName = s.SubscriptionPlan.Name,
                    StartDate = s.StartDate,
                    EndDate = s.EndDate,
                    IsActive = s.IsActive,
                    SubId = s.Id
                })
                .ToList();

            return View(subscriptions);
        }

        [HttpPost]
        public async Task<IActionResult> CancelSubscription(int subscriptionId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Challenge();

            var subscription = await _context.UserSubscriptions
                .Include(s => s.SubscriptionPlan)
                .FirstOrDefaultAsync(s => s.Id == subscriptionId && s.AppUserId == user.Id && s.IsActive);

            if (subscription == null)
            {
                TempData["ErrorMessage"] = "Subscription not found or already cancelled.";
                return RedirectToAction("MyPlans");
            }

            subscription.IsActive = false;
            _context.UserSubscriptions.Update(subscription);
            await _context.SaveChangesAsync();

            string subject = "Subscription Cancelled";
            string message = $@"
            <p>Hello <strong>{user.UserName}</strong>,</p>
            <p>Your subscription to the <strong>{subscription.SubscriptionPlan.Name}</strong> plan has been successfully cancelled.</p>
            <p>We're sorry to see you go. If you have any questions, feel free to contact us.</p>
            <br/>
            <p>Best regards,<br/>HoroScope Team</p>";

            await _emailService.SendMailAsync(user.Email, subject, message);

            TempData["SuccessMessage"] = "Subscription successfully cancelled.";

            return RedirectToAction("MyPlans");
        }

    }
}
