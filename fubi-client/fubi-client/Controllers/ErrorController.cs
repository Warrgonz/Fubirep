using Microsoft.AspNetCore.Mvc;

namespace fubi_client.Controllers
{
    public class ErrorController : Controller
    {
        [Route("error/404")]
        public IActionResult NotFound()
        {
            return View();
        }
    }
}
