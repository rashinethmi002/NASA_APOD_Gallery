using Microsoft.AspNetCore.Mvc;
using NASA_APOD_Gallery.Repositories;

namespace NASA_APOD_Gallery.Controllers
{
    public class GalleryController : Controller
    {
        private readonly ApodRepository _apodRepository;

        public GalleryController(ApodRepository apodRepository)
        {
            _apodRepository = apodRepository;
        }

        public async Task<IActionResult> Index()
        {
            // Read all saved APOD records from SQL Server database.
            var apodList = await _apodRepository.GetAllApodAsync();

            return View(apodList);
        }
    }
}