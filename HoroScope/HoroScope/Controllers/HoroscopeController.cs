using HoroScope.Services;
using Microsoft.AspNetCore.Mvc;

public class HoroscopeController : Controller
{
    private readonly NominatimService _nominatim;
    private readonly AstroService _astro;

    public HoroscopeController(NominatimService nominatim, AstroService astro)
    {
        _nominatim = nominatim;
        _astro = astro;
    }

    [HttpGet]
    public IActionResult Index() => View();

    [HttpPost]
    public async Task<IActionResult> Index(HoroscopeInputModel model)
    {
        if (string.IsNullOrEmpty(model.City))
        {
            ModelState.AddModelError("City", "Please enter a city name");
            return View(model);
        }


        var location = await _nominatim.GetCoordinatesAsync(model.City);
        if (location == null)
        {
            ModelState.AddModelError("City", "City not found");
            return View(model);
        }

        var birthDateTime = model.BirthDate.Date + model.BirthTime;

        var astroData = await _astro.GetAstroDataAsync(birthDateTime, location.Latitude, location.Longitude);

        if (astroData == null)
        {
            ModelState.AddModelError("", "Could not retrieve astrology data");
            return View(model);
        }

        return View("Result", astroData);
    }

}
