using HoroScope.DAL;
using HoroScope.Interfaces;
using HoroScope.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
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
                IsActive = true
            };

            _context.UserSubscriptions.Add(newSub);
            await _context.SaveChangesAsync();


            string subject = "Subscription Activated";
            string message = $"Hello {user.UserName},<br/><br/>Thank you for subscribing to our <b>{plan.Name}</b> plan.<br/>" +
                             $"Your subscription is now active until <b>{newSub.EndDate.ToShortDateString()}</b>.";


            await _emailService.SendMailAsync(user.Email, subject, message);

            return View("Success");
        }

        public IActionResult Cancel()
        {
            return View("Cancel");
        }

    }
}
