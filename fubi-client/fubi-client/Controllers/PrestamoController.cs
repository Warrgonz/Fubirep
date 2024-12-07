using fubi_client.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace fubi_client.Controllers
{
    public class PrestamoController : Controller
    {
        private readonly IHttpClientFactory _http;
        private readonly IConfiguration _conf;

        public PrestamoController(IHttpClientFactory http, IConfiguration conf)
        {
            _http = http;
            _conf = conf;
        }

        public IActionResult Index()
        {
            using (var client = _http.CreateClient())
            {
                // La URL de la API para obtener los préstamos
                string url = _conf.GetSection("Variables:UrlApi").Value + "Prestamos/ObtenerPrestamos";

                // Realizamos la solicitud GET al endpoint
                var response = client.GetAsync(url).Result;

                // Leemos la respuesta como un objeto de tipo 'Respuesta'
                var result = response.Content.ReadFromJsonAsync<Respuesta>().Result;

                if (result != null && result.Codigo == 0 && result.Contenido != null)
                {
                    // Deserializamos el contenido en una lista del modelo 'Prestamo'
                    var datosContenido = JsonSerializer.Deserialize<List<PrestamoRequest>>((JsonElement)result.Contenido);
                    return View(datosContenido);
                }

                // Si no hay datos o hubo un error, devolvemos una lista vacía
                return View(new List<PrestamoRequest>());
            }
        }


        [HttpGet]
        public IActionResult CreatePrestamo()
        {
            using (var client = _http.CreateClient())
            {
                // Obtenemos los beneficiarios
                string urlBeneficiarios = _conf.GetSection("Variables:UrlApi").Value + "Beneficiarios/ObtenerBeneficiarios";
                var responseBeneficiarios = client.GetAsync(urlBeneficiarios).Result;
                var resultBeneficiarios = responseBeneficiarios.Content.ReadFromJsonAsync<Respuesta>().Result;

                if (resultBeneficiarios != null && resultBeneficiarios.Codigo == 0)
                {
                    ViewBag.Beneficiarios = JsonSerializer.Deserialize<List<Beneficiarios>>((JsonElement)resultBeneficiarios.Contenido);
                }

                // Obtenemos todos los usuarios sin filtrar por rol
                string urlUsuarios = _conf.GetSection("Variables:UrlApi").Value + "User/ObtenerUsuarios";
                var responseUsuarios = client.GetAsync(urlUsuarios).Result;
                var resultUsuarios = responseUsuarios.Content.ReadFromJsonAsync<Respuesta>().Result;

                if (resultUsuarios != null && resultUsuarios.Codigo == 0)
                {
                    ViewBag.Encargados = JsonSerializer.Deserialize<List<User>>((JsonElement)resultUsuarios.Contenido);
                }

                // Obtenemos los inventarios
                string urlInventarios = _conf.GetSection("Variables:UrlApi").Value + "Inventario/ObtenerInventarios";
                var responseInventarios = client.GetAsync(urlInventarios).Result;
                var resultInventarios = responseInventarios.Content.ReadFromJsonAsync<Respuesta>().Result;

                if (resultInventarios != null && resultInventarios.Codigo == 0)
                {
                    ViewBag.Inventarios = JsonSerializer.Deserialize<List<Inventario>>((JsonElement)resultInventarios.Contenido);
                }

                // Simulación de estados del préstamo
                ViewBag.Estados = new List<dynamic>
        {
            new { Id = 1, Nombre = "Pendiente" },
            new { Id = 2, Nombre = "Entregado" },
            new { Id = 3, Nombre = "Devuelto" }
        };

                return View();
            }
        }


        [HttpPost]
        public async Task<IActionResult> CreatePrestamo(Prestamo model)
        {
            using (var client = _http.CreateClient())
            {
                var url = _conf.GetSection("Variables:UrlApi").Value + "Prestamos/CrearPrestamo";
                var prestamoContent = JsonContent.Create(model);

                var response = await client.PostAsync(url, prestamoContent);

                if (response.IsSuccessStatusCode)
                {
                    return RedirectToAction("Index");
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
        public async Task<IActionResult> EditPrestamo(int id)
        {
            using (var client = _http.CreateClient())
            {
                string apiUrl = _conf.GetSection("Variables:UrlApi").Value;

                try
                {
                    // Obtener el préstamo específico
                    var responsePrestamo = await client.GetAsync($"{apiUrl}Prestamos/ObtenerPrestamo/{id}");
                    if (!responsePrestamo.IsSuccessStatusCode)
                    {
                        ViewBag.ErrorMessage = "Error al cargar la información del préstamo.";
                        return RedirectToAction("Index");
                    }
                    var prestamo = await responsePrestamo.Content.ReadFromJsonAsync<PrestamoDetalle>();

                    // Obtenemos los beneficiarios
                    string urlBeneficiarios = _conf.GetSection("Variables:UrlApi").Value + "Beneficiarios/ObtenerBeneficiarios";
                    var responseBeneficiarios = client.GetAsync(urlBeneficiarios).Result;
                    var resultBeneficiarios = responseBeneficiarios.Content.ReadFromJsonAsync<Respuesta>().Result;

                    if (resultBeneficiarios != null && resultBeneficiarios.Codigo == 0)
                    {
                        ViewBag.Beneficiarios = JsonSerializer.Deserialize<List<Beneficiarios>>((JsonElement)resultBeneficiarios.Contenido);
                    }

                    // Obtenemos todos los usuarios sin filtrar por rol
                    string urlUsuarios = _conf.GetSection("Variables:UrlApi").Value + "User/ObtenerUsuarios";
                    var responseUsuarios = client.GetAsync(urlUsuarios).Result;
                    var resultUsuarios = responseUsuarios.Content.ReadFromJsonAsync<Respuesta>().Result;

                    if (resultUsuarios != null && resultUsuarios.Codigo == 0)
                    {
                        ViewBag.Encargados = JsonSerializer.Deserialize<List<User>>((JsonElement)resultUsuarios.Contenido);
                    }

                    // Obtenemos los inventarios
                    string urlInventarios = _conf.GetSection("Variables:UrlApi").Value + "Inventario/ObtenerInventarios";
                    var responseInventarios = client.GetAsync(urlInventarios).Result;
                    var resultInventarios = responseInventarios.Content.ReadFromJsonAsync<Respuesta>().Result;

                    if (resultInventarios != null && resultInventarios.Codigo == 0)
                    {
                        ViewBag.Inventarios = JsonSerializer.Deserialize<List<Inventario>>((JsonElement)resultInventarios.Contenido);
                    }

                    // Simulación de estados del préstamo
                    ViewBag.Estados = new List<dynamic>
        {
            new { Id = 1, Nombre = "Pendiente" },
            new { Id = 2, Nombre = "Entregado" },
            new { Id = 3, Nombre = "Devuelto" }
        };

                    return View(prestamo);
                }
                catch (Exception ex)
                {
                    ViewBag.ErrorMessage = $"Error inesperado: {ex.Message}";
                    return RedirectToAction("Index");
                }
            }
        }

        // POST: Guardar cambios del préstamo
        [HttpPost]
        public async Task<IActionResult> EditPrestamo(PrestamoDetalle model)
        {
            
            using (var client = _http.CreateClient())
            {
                string apiUrl = _conf.GetSection("Variables:UrlApi").Value + "Prestamo/ActualizarPrestamo";

                try
                {
                    // Transformar PrestamoDetalle a Prestamo
                    var prestamo = new Prestamo
                    {
                        id_prestamo = model.LoanID,
                        id_beneficiario = Convert.ToInt32(model.BeneficiaryName),
                        id_encargado = Convert.ToInt32(model.ManagerName),
                        id_inventario = Convert.ToInt32(model.ItemName),
                        cantidad = model.LoanQuantity,
                        fecha_limite_devolución = model.LoanDueDate,
                        id_estado = int.Parse(model.LoanStatus) // ID del estado
                    };

                    // Serializar y enviar al API
                    var jsonContent = new StringContent(JsonSerializer.Serialize(prestamo), Encoding.UTF8, "application/json");
                    var response = await client.PutAsync(apiUrl, jsonContent);

                    if (response.IsSuccessStatusCode)
                        return RedirectToAction("Index");

                    ViewBag.ErrorMessage = "Error al actualizar el préstamo.";
                    return View(model);
                }
                catch (Exception ex)
                {
                    ViewBag.ErrorMessage = $"Error inesperado: {ex.Message}";
                    return View(model);
                }
            }
        }
        [HttpPost]
        public async Task<IActionResult> DeletePrestamo(int id)
        {
            using (var client = _http.CreateClient())
            {
                var url = _conf.GetSection("Variables:UrlApi").Value + $"Prestamo/EliminarPrestamo/{id}";
                var response = await client.DeleteAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    return RedirectToAction("Index");
                }
                else
                {
                    ViewBag.ErrorMessage = "Hubo un error al eliminar el préstamo.";
                    return RedirectToAction("Index");
                }
            }
        }

    }
}
