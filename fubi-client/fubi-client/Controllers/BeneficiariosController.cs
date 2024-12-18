using Microsoft.AspNetCore.Mvc;
using fubi_client.Models;
using System.Text.Json;
using System.Net.Http.Json;

namespace fubi_client.Controllers
{
    public class BeneficiariosController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public BeneficiariosController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        // GET: Beneficiarios/Index
        public IActionResult Index()
        {
            using (var client = _httpClientFactory.CreateClient())
            {
                // Construye la URL de la API para obtener beneficiarios
                string url = _configuration["Variables:UrlApi"] + "Beneficiarios/ObtenerBeneficiarios";
                var response = client.GetAsync(url).Result;

                if (response.IsSuccessStatusCode)
                {
                    var result = response.Content.ReadFromJsonAsync<Respuesta>().Result;

                    if (result != null && result.Codigo == 0 && result.Contenido != null)
                    {
                        var beneficiarios = JsonSerializer.Deserialize<List<Beneficiarios>>((JsonElement)result.Contenido);
                        return View(beneficiarios);
                    }
                }

                ViewBag.ErrorMessage = "Hubo un problema al cargar los beneficiarios.";
                return View(new List<Beneficiarios>());
            }
        }

        // GET: Beneficiarios/Create
        [HttpGet]
        public IActionResult CreateBeneficiarios()
        {
            return View();
        }

        // POST: Beneficiarios/Create
        [HttpPost]
        public async Task<IActionResult> CreateBeneficiarios(Beneficiarios model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            using (var client = _httpClientFactory.CreateClient())
            {
                string url = _configuration["Variables:UrlApi"] + "Beneficiarios/CreateBeneficiarios";
                var response = await client.PostAsJsonAsync(url, model);

                if (response.IsSuccessStatusCode)
                {
                    return RedirectToAction("Index");
                }

                var result = await response.Content.ReadFromJsonAsync<Respuesta>();
                ViewBag.ErrorMessage = result?.Mensaje ?? "Hubo un error al crear el beneficiario.";
                return View(model);
            }
        }

        // GET: Beneficiarios/Edit/{id}
        [HttpGet]
        public IActionResult EditBeneficiarios(int id)
        {
            using (var client = _httpClientFactory.CreateClient())
            {
                string url = _configuration["Variables:UrlApi"] + "Beneficiarios/ObtenerBeneficiarios";
                var response = client.GetAsync(url).Result;

                if (response.IsSuccessStatusCode)
                {
                    var result = response.Content.ReadFromJsonAsync<Respuesta>().Result;

                    if (result != null && result.Codigo == 0 && result.Contenido != null)
                    {
                        var beneficiarios = JsonSerializer.Deserialize<List<Beneficiarios>>((JsonElement)result.Contenido);
                        var beneficiario = beneficiarios?.FirstOrDefault(b => b.id_beneficiario == id);

                        if (beneficiarios != null)
                        {
                            return View(beneficiarios);
                        }
                    }
                }

                ViewBag.ErrorMessage = "No se encontró el beneficiario.";
                return RedirectToAction("Index");
            }
        }

        // POST: Beneficiarios/Edit
        [HttpPost]
        public async Task<IActionResult> EditBeneficiarios(Beneficiarios model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            using (var client = _httpClientFactory.CreateClient())
            {
                string url = _configuration["Variables:UrlApi"] + "Beneficiarios/ActualizarBeneficiario";
                var response = await client.PutAsJsonAsync(url, model);

                if (response.IsSuccessStatusCode)
                {
                    return RedirectToAction("Index");
                }

                ViewBag.ErrorMessage = "Hubo un error al actualizar el beneficiario.";
                return View(model);
            }
        }

        // GET: Beneficiarios/Disable/{id}
        [HttpGet]
        public IActionResult DeshabilitarBeneficiario(int id)
        {
            using (var client = _httpClientFactory.CreateClient())
            {
                string url = _configuration["Variables:UrlApi"] + "Beneficiarios/ObtenerBeneficiarios";
                var response = client.GetAsync(url).Result;

                if (response.IsSuccessStatusCode)
                {
                    var result = response.Content.ReadFromJsonAsync<Respuesta>().Result;

                    if (result != null && result.Codigo == 0 && result.Contenido != null)
                    {
                        var beneficiarios = JsonSerializer.Deserialize<List<Beneficiarios>>((JsonElement)result.Contenido);
                        var beneficiario = beneficiarios?.FirstOrDefault(b => b.id_beneficiario == id);

                        if (beneficiario != null)
                        {
                            return View(beneficiario);
                        }
                    }
                }

                ViewBag.ErrorMessage = "No se encontró el beneficiario.";
                return RedirectToAction("Index");
            }
        }

        // POST: Beneficiarios/Disable
        [HttpPost]
        public async Task<IActionResult> DeshabilitarBeneficiario(Beneficiarios model)
        {
            using (var client = _httpClientFactory.CreateClient())
            {
                string url = _configuration["Variables:UrlApi"] + "Beneficiarios/DeshabilitarBeneficiario";
                var response = await client.PutAsJsonAsync(url, model);

                if (response.IsSuccessStatusCode)
                {
                    return RedirectToAction("Index");
                }

                ViewBag.ErrorMessage = "Hubo un error al deshabilitar el beneficiario.";
                return View(model);
            }
        }
    }
}

