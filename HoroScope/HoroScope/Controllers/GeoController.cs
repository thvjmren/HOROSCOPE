using HoroScope.Models;
using HoroScope.Services;
using Microsoft.AspNetCore.Mvc;

namespace HoroScope.Controllers
{
    public class GeoController : Controller
    {
        private readonly NominatimService _nominatimService;

        public GeoController(NominatimService nominatimService)
        {
            _nominatimService = nominatimService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Index(string city)
        {
            if (string.IsNullOrWhiteSpace(city))
            {
                ModelState.AddModelError("", "Please enter a city name.");
                return View();
            }

            GeoLocation? location = await _nominatimService.GetCoordinatesAsync(city);

            if (location == null)
            {
                ViewBag.Error = "Location not found.";
                return View();
            }

            return View("Result", location);
        }
    }
}
