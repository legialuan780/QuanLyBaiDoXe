using Microsoft.AspNetCore.Mvc;

namespace QuanLyBaiDoXe.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class VehicleVisionController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
