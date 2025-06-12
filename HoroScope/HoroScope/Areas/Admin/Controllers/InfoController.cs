//using HoroScope.DAL;
//using HoroScope.Models;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.EntityFrameworkCore;

//namespace HoroScope.Areas.Admin.Controllers
//{
//    [Area("Admin")]
//    public class InfoController : Controller
//    {
//        private readonly AppDbContext _context;

//        public InfoController(AppDbContext context)
//        {
//            _context = context;
//        }
//        public async Task<IActionResult> Index()
//        {
//            var info = await _context.Infos.FirstOrDefaultAsync();

//            return View(info);
//        }

//        [HttpPost]
//        public async Task<IActionResult> Index(GetInfoVM model)
//        {
//            var existing = await _context.Infos.FirstOrDefaultAsync();

//            if (existing == null)
//            {
//                _context.Infos.Add(model);
//            }
//            else
//            {
//                existing.Phone = model.Phone;
//                existing.Email = model.Email;
//                existing.Address = model.Address;
//                existing.FacebookUrl = model.FacebookUrl;
//                existing.TwitterUrl = model.TwitterUrl;
//                existing.InstagramUrl = model.InstagramUrl;
//                existing.LinkedinUrl = model.LinkedinUrl;

//                _context.Infos.Update(existing);
//            }

//            await _context.SaveChangesAsync();
//            return RedirectToAction(nameof(Index));
//        }
//    }
//}
