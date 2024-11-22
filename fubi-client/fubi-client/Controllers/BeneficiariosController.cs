using Microsoft.AspNetCore.Mvc;
using fubi_client.Models;
using System.Text.Json;

namespace fubi_client.Controllers
{
    public class BeneficiariosController : Controller
    {
        private readonly IHttpClientFactory _http;
        private readonly IConfiguration _conf;

        public BeneficiariosController(IHttpClientFactory http, IConfiguration conf)
        {
            _http = http;
            _conf = conf;
        }

        public async Task<IActionResult> Index()
        {
            using (var client = _http.CreateClient())
            {
                var url = $"{_conf.GetSection("Variables:UrlApi").Value}Beneficiarios/GetAll";
                var response = await client.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var data = await response.Content.ReadFromJsonAsync<List<Beneficiarios>>();
                    return View(data);
                }

                ViewBag.ErrorMessage = "No se pudieron cargar los beneficiarios.";
                return View(new List<Beneficiarios>());
            }
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Beneficiarios model)
        {
            using (var client = _http.CreateClient())
            {
                var url = $"{_conf.GetSection("Variables:UrlApi").Value}Beneficiarios/Create";
                var content = JsonContent.Create(model);

                var response = await client.PostAsync(url, content);

                if (response.IsSuccessStatusCode)
                {
                    return RedirectToAction("Index");
                }

                ViewBag.ErrorMessage = "Error al crear el beneficiario.";
                return View(model);
            }
        }
    }
}
