using Microsoft.AspNetCore.Mvc;
using static System.Net.WebRequestMethods;
using fubi_client.Models;
using System.Text.Json;
using System.Reflection;
using System.Net.Http.Headers;

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

        public IActionResult Index()
        {
            using (var client = _http.CreateClient())
            {
                // URL de la API para obtener la lista de beneficiarios
                string url = _conf.GetSection("Variables:UrlApi").Value + "Beneficiarios/ObtenerBeneficiarios";

                var response = client.GetAsync(url).Result;
                var result = response.Content.ReadFromJsonAsync<Respuesta>().Result;

                if (result != null && result.Codigo == 0 && result.Contenido != null)
                {
                    var datosContenido = JsonSerializer.Deserialize<List<Beneficiarios>>((JsonElement)result.Contenido);
                    return View(new List<Beneficiarios>(datosContenido));
                }

                return View(new List<Beneficiarios>());
            }
        }

        [HttpGet]
        public IActionResult CreateBeneficiario()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateBeneficiario(Beneficiarios model)
        {
            using (var client = _http.CreateClient())
            {
                var url = _conf.GetSection("Variables:UrlApi").Value + "Beneficiarios/CreateBeneficiario";
                var beneficiarioContent = JsonContent.Create(model);

                var response = await client.PostAsync(url, beneficiarioContent);

                if (response.IsSuccessStatusCode)
                {
                    return RedirectToAction("Index", "Beneficiarios");
                }
                else
                {
                    var result = await response.Content.ReadFromJsonAsync<Respuesta>();
                    ViewBag.ErrorMessage = result?.Mensaje ?? "Hubo un error interno";
                    return View(model);
                }
            }
        }

        [HttpGet]
        public IActionResult EditBeneficiario(int id)
        {
            using (var client = _http.CreateClient())
            {
                var url = _conf.GetSection("Variables:UrlApi").Value + $"Beneficiarios/ObtenerBeneficiarios";

                var response = client.GetAsync(url).Result;
                var result = response.Content.ReadFromJsonAsync<Respuesta>().Result;

                if (result != null && result.Codigo == 0)
                {
                    var beneficiario = JsonSerializer.Deserialize<List<Beneficiarios>>((JsonElement)result.Contenido)?.FirstOrDefault(b => b.Id == id);
                    return View(beneficiario);
                }

                ViewBag.ErrorMessage = "No se encontró el beneficiario.";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public async Task<IActionResult> EditBeneficiario(Beneficiarios model)
        {
            using (var client = _http.CreateClient())
            {
                var url = _conf.GetSection("Variables:UrlApi").Value + "Beneficiarios/ActualizarBeneficiario";
                var beneficiarioContent = JsonContent.Create(model);

                var response = await client.PutAsync(url, beneficiarioContent);

                if (response.IsSuccessStatusCode)
                {
                    return RedirectToAction("Index", "Beneficiarios");
                }
                else
                {
                    ViewBag.ErrorMessage = "Hubo un error al actualizar el beneficiario.";
                    return View(model);
                }
            }
        }

        [HttpGet]
        public IActionResult DesabilitarBeneficiario() {
            return View();
        
        }
    }
}


