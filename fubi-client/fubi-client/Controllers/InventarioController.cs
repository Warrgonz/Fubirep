using Microsoft.AspNetCore.Mvc;
using fubi_client.Models;
using System.Text.Json;

namespace fubi_client.Controllers
{
    public class InventarioController : Controller
    {
        private readonly IHttpClientFactory _http;
        private readonly IConfiguration _conf;

        public InventarioController(IHttpClientFactory http, IConfiguration conf)
        {
            _http = http;
            _conf = conf;
        }

        public IActionResult Index()
        {
            using (var client = _http.CreateClient())
            {
                // La URL de la API donde se obtiene la lista de inventarios
                string url = _conf.GetSection("Variables:UrlApi").Value + "Inventario/ObtenerInventarios";

                // Realizamos la solicitud GET al endpoint de la API
                var response = client.GetAsync(url).Result;

                // Leemos la respuesta como un objeto de tipo 'Respuesta'
                var result = response.Content.ReadFromJsonAsync<Respuesta>().Result;

                // Si la respuesta es exitosa y tiene datos
                if (result != null && result.Codigo == 0 && result.Contenido != null)
                {
                    // Deserializamos el contenido en una lista de inventarios
                    var datosContenido = JsonSerializer.Deserialize<List<Inventario>>((JsonElement)result.Contenido);

                    // Devolvemos la vista con los inventarios obtenidos
                    return View(new List<Inventario>(datosContenido));
                }

                // Si no hay inventarios o la respuesta fue incorrecta, devolvemos una lista vacía
                return View(new List<Inventario>());
            }
        }


        [HttpGet]
        public IActionResult CreateInventario()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateInventario(Inventario model)
        {
            using (var client = _http.CreateClient())
            {
                model.ruta_imagen = "";

                var url = _conf.GetSection("Variables:UrlApi").Value + "Inventario/CreateInventario";
                var inventarioContent = JsonContent.Create(model);

                var response = await client.PostAsync(url, inventarioContent);

                if (response.IsSuccessStatusCode)
                {
                    return RedirectToAction("Index", "Inventario");
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

        [HttpGet]
        public IActionResult EditInventario(int id)
        {
            using (var client = _http.CreateClient())
            {
                var url = _conf.GetSection("Variables:UrlApi").Value + $"Inventario/ObtenerInventarios";

                var response = client.GetAsync(url).Result;
                var result = response.Content.ReadFromJsonAsync<Respuesta>().Result;

                if (result != null && result.Codigo == 0)
                {
                    var inventario = JsonSerializer.Deserialize<List<Inventario>>((JsonElement)result.Contenido)?.FirstOrDefault(i => i.id_inventario == id);
                    return View(inventario);
                }

                ViewBag.ErrorMessage = "No se encontró el inventario.";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public async Task<IActionResult> EditInventario(Inventario model)
        {
            using (var client = _http.CreateClient())
            {
                var url = _conf.GetSection("Variables:UrlApi").Value + "Inventario/ActualizarInventario";
                var inventarioContent = JsonContent.Create(model);

                var response = await client.PutAsync(url, inventarioContent);

                if (response.IsSuccessStatusCode)
                {
                    return RedirectToAction("Index", "Inventario");
                }
                else
                {
                    ViewBag.ErrorMessage = "Hubo un error al actualizar el inventario.";
                    return View(model);
                }
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteInventario(int id)
        {
            using (var client = _http.CreateClient())
            {
                var url = _conf.GetSection("Variables:UrlApi").Value + $"Inventario/EliminarInventario/{id}";
                var response = await client.DeleteAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    return RedirectToAction("Index", "Inventario");
                }
                else
                {
                    ViewBag.ErrorMessage = "Hubo un error al eliminar el inventario.";
                    return RedirectToAction("Index");
                }
            }
        }


    }
}