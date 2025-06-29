using HoroScope.DAL;
using HoroScope.Models;
using HoroScope.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HoroScope.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class ContactController : Controller
    {
        private readonly AppDbContext _context;

        public ContactController(AppDbContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index()
        {
            List<GetContactVM> contactVMs = await _context.Contacts.Select(c => new GetContactVM
            {
                Id = c.Id,
                Name = c.Name,
                Email = c.Email,
                Message = c.Message,
                CreatedAt = c.CreatedAt
            }).ToListAsync();

            return View(contactVMs);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id is null || id <= 0) return BadRequest();

            Contact? contact = await _context.Contacts.FirstOrDefaultAsync(sc => sc.Id == id);

            if (contact is null) return NotFound();

            GetContactVM contactVM = new()
            {
                Id = contact.Id,
                Name = contact.Name,
                Email = contact.Email,
                Message = contact.Message,
                CreatedAt = contact.CreatedAt
            };

            return View(contactVM);
        }
    }
}
