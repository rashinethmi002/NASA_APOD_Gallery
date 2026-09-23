using Microsoft.AspNetCore.Mvc;
using NASA_APOD_Gallery.Repositories;
using NASA_APOD_Gallery.Services;

namespace NASA_APOD_Gallery.Controllers
{
    public class HomeController : Controller
    {
        private readonly NasaApodService _nasaApodService;
        private readonly ApodRepository _apodRepository;

        public HomeController(
            NasaApodService nasaApodService,
            ApodRepository apodRepository)
        {
            _nasaApodService = nasaApodService;
            _apodRepository = apodRepository;
        }

        public async Task<IActionResult> Index()
        {
            var apodList =
                await _apodRepository.GetAllApodAsync();

            return View(apodList);
        }

        [HttpPost]
        public async Task<IActionResult> FetchApod(
            DateTime startDate,
            DateTime endDate)
        {
            try
            {
                // Fetch and save data (service handles fallback to per-day if needed).
                await _nasaApodService.FetchAndSaveApodAsync(startDate, endDate);

                TempData["Success"] = "APOD data fetched and saved successfully.";

                // Immediately load saved records and return the Index view so the
                // gallery displays the freshly saved images without requiring an extra refresh.
                var apodList = await _apodRepository.GetAllApodAsync();
                return View("Index", apodList);
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                // On error, show the existing records page with the error message.
                var apodList = await _apodRepository.GetAllApodAsync();
                return View("Index", apodList);
            }
        }
    }
}