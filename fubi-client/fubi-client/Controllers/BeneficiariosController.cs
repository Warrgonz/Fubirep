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

        // Mostrar todos los beneficiarios
        public async Task<IActionResult> Index()
        {
            using (var client = _http.CreateClient())
            {
                var url = _conf.GetSection("Variables:UrlApi").Value + "Beneficiarios/GetAllBeneficiarios";
                var response = await client.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<List<Beneficiarios>>();
                    return View(result);
                }
                else
                {
                    ViewBag.ErrorMessage = "Hubo un error al cargar los beneficiarios";
                    return View(new List<Beneficiarios>());
                }
            }
        }

        // Crear un nuevo beneficiario (GET)
        [HttpGet]
        public IActionResult CreateBeneficiario()
        {
            return View();
        }

        // Crear un nuevo beneficiario (POST)
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
                    return RedirectToAction("Index");
                }
                else
                {
                    var result = await response.Content.ReadFromJsonAsync<Respuesta>();
                    ViewBag.ErrorMessage = result?.Codigo == -2 ? result.Mensaje : "Hubo un error interno";
                    return View(model);
                }
            }
        }

        // Editar un beneficiario (GET)
        [HttpGet]
        public async Task<IActionResult> EditBeneficiario(int id)
        {
            using (var client = _http.CreateClient())
            {
                var url = _conf.GetSection("Variables:UrlApi").Value + $"Beneficiarios/GetBeneficiario/{id}";
                var response = await client.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<Beneficiarios>();
                    return View(result);
                }
                else
                {
                    ViewBag.ErrorMessage = "Hubo un error al cargar los datos del beneficiario";
                    return RedirectToAction("Index");
                }
            }
        }

        // Editar un beneficiario (POST)
        [HttpPost]
        public async Task<IActionResult> EditBeneficiario(Beneficiarios model)
        {
            using (var client = _http.CreateClient())
            {
                var url = _conf.GetSection("Variables:UrlApi").Value + "Beneficiarios/UpdateBeneficiario";
                var beneficiarioContent = JsonContent.Create(model);

                var response = await client.PutAsync(url, beneficiarioContent);

                if (response.IsSuccessStatusCode)
                {
                    return RedirectToAction("Index");
                }
                else
                {
                    ViewBag.ErrorMessage = "Hubo un error al actualizar el beneficiario";
                    return View(model);
                }
            }
        }

        // Eliminar un beneficiario
        public async Task<IActionResult> DeleteBeneficiario(int id)
        {
            using (var client = _http.CreateClient())
            {
                var url = _conf.GetSection("Variables:UrlApi").Value + $"Beneficiarios/DeleteBeneficiario/{id}";
                var response = await client.DeleteAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    return RedirectToAction("Index");
                }
                else
                {
                    ViewBag.ErrorMessage = "Hubo un error al eliminar el beneficiario";
                    return RedirectToAction("Index");
                }
            }
        }
    }
}
