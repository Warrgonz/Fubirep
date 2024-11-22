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

        // Acción para listar los beneficiarios
        public async Task<IActionResult> Index()
        {
            using (var client = _http.CreateClient())
            {
                var url = _conf.GetSection("Variables:UrlApi").Value + "Beneficiarios/GetAll";
                var response = await client.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var beneficiarios = await response.Content.ReadFromJsonAsync<List<Beneficiarios>>();
                    return View(beneficiarios);
                }
                else
                {
                    ViewBag.ErrorMessage = "No se pudo cargar la lista de beneficiarios.";
                    return View(new List<Beneficiarios>());
                }
            }
        }

        // Acción para mostrar el formulario de creación
        [HttpGet]
        public IActionResult CreateBeneficiario()
        {
            return View();
        }

        // Acción para crear un beneficiario
        [HttpPost]
        public async Task<IActionResult> CreateBeneficiario(Beneficiarios model, IFormFile ruta_imagen)
        {
            using (var client = _http.CreateClient())
            {
                // 1. Crear beneficiario sin imagen
                var url = _conf.GetSection("Variables:UrlApi").Value + "Beneficiarios/Create";
                var beneficiarioContent = JsonContent.Create(model);

                var response = await client.PostAsync(url, beneficiarioContent);

                if (response.IsSuccessStatusCode)
                {
                    // 2. Subir imagen
                    if (ruta_imagen != null && ruta_imagen.Length > 0)
                    {
                        var imageUrl = $"{_conf.GetSection("Variables:UrlApi").Value}Beneficiarios/UploadImage/{model.Id}";
                        var formContent = new MultipartFormDataContent();
                        formContent.Add(new StreamContent(ruta_imagen.OpenReadStream()), "file", ruta_imagen.FileName);

                        var uploadResponse = await client.PostAsync(imageUrl, formContent);

                        if (uploadResponse.IsSuccessStatusCode)
                        {
                            return RedirectToAction("Index", "Beneficiarios");
                        }
                        else
                        {
                            ViewBag.ErrorMessage = "No se pudo cargar la imagen.";
                            return View(model);
                        }
                    }

                    return RedirectToAction("Index", "Beneficiarios");
                }
                else
                {
                    var result = await response.Content.ReadFromJsonAsync<Respuesta>();

                    if (result.Codigo == -2)
                    {
                        ViewBag.ErrorMessage = result.Mensaje;
                    }
                    else
                    {
                        ViewBag.ErrorMessage = "Hubo un error interno";
                    }
                    return View(model);
                }
            }
        }

        // Acción para mostrar el formulario de edición
        [HttpGet]
        public async Task<IActionResult> EditBeneficiario(int id)
        {
            using (var client = _http.CreateClient())
            {
                var url = $"{_conf.GetSection("Variables:UrlApi").Value}Beneficiarios/GetById/{id}";
                var response = await client.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var beneficiario = await response.Content.ReadFromJsonAsync<Beneficiarios>();
                    return View(beneficiario);
                }
                else
                {
                    ViewBag.ErrorMessage = "No se pudo cargar el beneficiario.";
                    return RedirectToAction("Index");
                }
            }
        }

        // Acción para actualizar un beneficiario
        [HttpPost]
        public async Task<IActionResult> EditBeneficiario(Beneficiarios model, IFormFile ruta_imagen)
        {
            using (var client = _http.CreateClient())
            {
                // 1. Actualizar beneficiario
                var url = _conf.GetSection("Variables:UrlApi").Value + "Beneficiarios/Update";
                var beneficiarioContent = JsonContent.Create(model);

                var response = await client.PutAsync(url, beneficiarioContent);

                if (response.IsSuccessStatusCode)
                {
                    // 2. Subir nueva imagen si se proporcionó
                    if (ruta_imagen != null && ruta_imagen.Length > 0)
                    {
                        var imageUrl = $"{_conf.GetSection("Variables:UrlApi").Value}Beneficiarios/UploadImage/{model.Id}";
                        var formContent = new MultipartFormDataContent();
                        formContent.Add(new StreamContent(ruta_imagen.OpenReadStream()), "file", ruta_imagen.FileName);

                        var uploadResponse = await client.PostAsync(imageUrl, formContent);

                        if (!uploadResponse.IsSuccessStatusCode)
                        {
                            ViewBag.ErrorMessage = "No se pudo actualizar la imagen.";
                            return View(model);
                        }
                    }

                    return RedirectToAction("Index", "Beneficiarios");
                }
                else
                {
                    var result = await response.Content.ReadFromJsonAsync<Respuesta>();
                    ViewBag.ErrorMessage = result?.Mensaje ?? "Hubo un error interno.";
                    return View(model);
                }
            }
        }

        // Acción para eliminar un beneficiario
        [HttpPost]
        public async Task<IActionResult> DeleteBeneficiario(int id)
        {
            using (var client = _http.CreateClient())
            {
                var url = $"{_conf.GetSection("Variables:UrlApi").Value}Beneficiarios/Delete/{id}";
                var response = await client.DeleteAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    return RedirectToAction("Index", "Beneficiarios");
                }
                else
                {
                    ViewBag.ErrorMessage = "No se pudo eliminar el beneficiario.";
                    return RedirectToAction("Index");
                }
            }
        }
    }
}
