using Microsoft.AspNetCore.Mvc;

namespace MainBackend.Controllers
{
    public class ErrorsController : Controller
    {
        public IActionResult NotFoundPage()
        {
            return View();
        }
    }
}
