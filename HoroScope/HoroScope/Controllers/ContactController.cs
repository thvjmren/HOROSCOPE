using HoroScope.DAL;
using HoroScope.Models;
using HoroScope.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HoroScope.Controllers
{
    public class ContactController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        public ContactController(AppDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }
        public async Task<IActionResult> Index()
        {
            var settings = await _context.Settings.ToListAsync();
            //var contact = await _context.Contact.FirstOrDefaultAsync();

            var vm = new ContactVM
            {
                Phone = settings.FirstOrDefault(s => s.Key == "Phone")?.Value,
                Email = settings.FirstOrDefault(s => s.Key == "Email")?.Value,
                Address = settings.FirstOrDefault(s => s.Key == "Address")?.Value,
            };

            AppUser appUser;

            if (User.Identity.IsAuthenticated)
            {
                appUser = await _userManager.FindByNameAsync(User.Identity.Name);
                ViewBag.Email = appUser.Email;
                ViewBag.Name = appUser.Name;
            }

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Contact(ContactVM vm)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction("Index", vm);
            }

            var contact = new Contact
            {
                Name = vm.Name,
                Email = vm.Email,
                Message = vm.Message,
                CreatedAt = DateTime.Now
            };

            _context.Contacts.Add(contact);
            _context.SaveChanges();

            TempData["Success"] = "Your message has been sent successfully!";
            return RedirectToAction("Index", vm);
        }


    }
}
