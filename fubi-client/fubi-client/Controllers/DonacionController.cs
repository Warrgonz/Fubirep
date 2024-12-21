using fubi_client.Models;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace fubi_client.Controllers
{
    public class DonacionController : Controller
    {
        private readonly IHttpClientFactory _http;
        private readonly IConfiguration _conf;

        public DonacionController(IHttpClientFactory http, IConfiguration conf)
        {
            _http = http;
            _conf = conf;
        }

        public IActionResult Index()
        {
            using (var client = _http.CreateClient())
            {
                string url = _conf.GetSection("Variables:UrlApi").Value + "Donacion/ObtenerDonaciones";
                var response = client.GetAsync(url).Result;
                var result = response.Content.ReadFromJsonAsync<Respuesta>().Result;

                if (result != null && result.Codigo == 0)
                {
                    var datosContenido = JsonSerializer.Deserialize<List<DonacionExtendida>>((JsonElement)result.Contenido);
                    return View(datosContenido);
                }

                return View(new List<DonacionExtendida>());
            }
        }


        public async Task<IActionResult> CreateDonacion()
        {
            using (var client = _http.CreateClient())
            {
                // Obtener la lista de tipos de movimiento
                var urlTiposMovimiento = _conf.GetSection("Variables:UrlApi").Value + "Donacion/ObtenerTiposMovimiento";
                var responseTiposMovimiento = await client.GetAsync(urlTiposMovimiento);
                var respuestaTiposMovimiento = await responseTiposMovimiento.Content.ReadFromJsonAsync<Respuesta>();
                var tiposMovimiento = new List<TipoMovimiento>();
                if (respuestaTiposMovimiento != null && respuestaTiposMovimiento.Codigo == 0)
                {
                    tiposMovimiento = JsonSerializer.Deserialize<List<TipoMovimiento>>(respuestaTiposMovimiento.Contenido.ToString());
                }

                // Obtener la lista de tipos de donación
                var urlTiposDonacion = _conf.GetSection("Variables:UrlApi").Value + "Donacion/ObtenerTiposDonacion";
                var responseTiposDonacion = await client.GetAsync(urlTiposDonacion);
                var respuestaTiposDonacion = await responseTiposDonacion.Content.ReadFromJsonAsync<Respuesta>();
                var tiposDonacion = new List<TipoDonacion>();
                if (respuestaTiposDonacion != null && respuestaTiposDonacion.Codigo == 0)
                {
                    tiposDonacion = JsonSerializer.Deserialize<List<TipoDonacion>>(respuestaTiposDonacion.Contenido.ToString());
                }

                // Obtener la lista de beneficiarios
                var urlBeneficiarios = _conf.GetSection("Variables:UrlApi").Value + "Donacion/ObtenerBeneficiarios";
                var responseBeneficiarios = await client.GetAsync(urlBeneficiarios);
                var respuestaBeneficiarios = await responseBeneficiarios.Content.ReadFromJsonAsync<Respuesta>();
                var beneficiarios = new List<Beneficiario>();
                if (respuestaBeneficiarios != null && respuestaBeneficiarios.Codigo == 0)
                {
                    beneficiarios = JsonSerializer.Deserialize<List<Beneficiario>>(respuestaBeneficiarios.Contenido.ToString());
                }

                // Obtener la lista de inventarios
                var urlInventarios = _conf.GetSection("Variables:UrlApi").Value + "Donacion/ObtenerInventarios";
                var responseInventarios = await client.GetAsync(urlInventarios);
                var respuestaInventarios = await responseInventarios.Content.ReadFromJsonAsync<Respuesta>();
                var inventarios = new List<Inventario>();
                if (respuestaInventarios != null && respuestaInventarios.Codigo == 0)
                {
                    inventarios = JsonSerializer.Deserialize<List<Inventario>>(respuestaInventarios.Contenido.ToString());
                }

                // Pasar los datos a la vista
                ViewBag.TiposMovimiento = tiposMovimiento;
                ViewBag.TiposDonacion = tiposDonacion;
                ViewBag.Beneficiarios = beneficiarios;
                ViewBag.Inventarios = inventarios;

                return View();
            }
        }


        [HttpPost]
        public async Task<IActionResult> CreateDonacion(Donacion model)
        {
            using (var client = _http.CreateClient())
            {
                var url = _conf.GetSection("Variables:UrlApi").Value + "Donacion/CreateDonacion";
                var donacionContent = JsonContent.Create(model);

                var response = await client.PostAsync(url, donacionContent);

                if (response.IsSuccessStatusCode)
                {
                    return RedirectToAction("Index");
                }
                else
                {
                    ViewBag.ErrorMessage = "Error al registrar la donación.";
                    return View(model);
                }
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteDonacion(int id)
        {
            using (var client = _http.CreateClient())
            {
                var url = _conf.GetSection("Variables:UrlApi").Value + $"Donacion/EliminarDonacion/{id}";
                var response = await client.DeleteAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    return RedirectToAction("Index", "Donacion");
                }
                else
                {
                    ViewBag.ErrorMessage = "Hubo un error al eliminar la donación.";
                    return RedirectToAction("Index");
                }
            }
        }


    }
}
