using fubi_client.Models;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Text;
using System.Text.Json;

namespace fubi_client.Controllers
{
    public class ErrorController : Controller
    {

        private readonly IHttpClientFactory _http;
        private readonly IConfiguration _conf;

        public ErrorController(IHttpClientFactory http, IConfiguration conf)
        {
            _http = http;
            _conf = conf;
        }

        [Route("error/404")]
        public IActionResult NotFound()
        {
            return View();
        }

        [Route("error/401")]
        public IActionResult Gone()
        {
            return View();
        }

        public IActionResult MostrarError()
        {
            return View("Error");
        }
    }
}
