using Microsoft.AspNetCore.Mvc;
using fubi_client.Models;
using System.Text.Json;
using System.Net.Http.Json;
using static System.Net.WebRequestMethods;
using Microsoft.AspNetCore.Mvc.Filters;
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

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var sesionData = HttpContext.Session.GetInt32("RolUsuario");

            if (sesionData == null || sesionData != 2)
            {
                // Redirige si el usuario no tiene el rol adecuado
                context.Result = RedirectToAction("NotFound", "Error");
            }

            base.OnActionExecuting(context);
        }

        // GET: Tabla de beneficiarios
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            using (var client = _http.CreateClient())
            {
                var url = _conf.GetSection("Variables:UrlApi").Value + "Beneficiarios/ObtenerBeneficiarios";

                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", HttpContext.Session.GetString("TokenUsuario"));

                var response = await client.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    // Leer la respuesta como un objeto de tipo Respuesta
                    var apiResponse = await response.Content.ReadFromJsonAsync<Respuesta>();

                    if (apiResponse != null && apiResponse.Codigo == 0)
                    {
                        try
                        {
                            // Deserializar directamente el contenido de la respuesta como una lista de Beneficiarios
                            var beneficiarios = JsonSerializer.Deserialize<List<Beneficiarios>>(apiResponse.Contenido.ToString());

                            // Verificar si la lista contiene datos
                            if (beneficiarios != null && beneficiarios.Any())
                            {
                                return View(beneficiarios);
                            }
                            else
                            {
                                TempData["ErrorMessage"] = "No hay beneficiarios registrados en este momento.";
                                return View(new List<Beneficiarios>());
                            }
                        }
                        catch (JsonException ex)
                        {
                            // Capturar errores de deserialización
                            TempData["ErrorMessage"] = $"Error al procesar los datos de los beneficiarios: {ex.Message}";
                            return View(new List<Beneficiarios>());
                        }
                    }
                    else
                    {
                        TempData["ErrorMessage"] = apiResponse?.Mensaje ?? "Error al obtener los beneficiarios.";
                        return View(new List<Beneficiarios>());
                    }
                }
                else
                {
                    TempData["ErrorMessage"] = "Hubo un error al obtener los beneficiarios.";
                    return View(new List<Beneficiarios>());
                }
            }
        }




        // GET: Formulario para agregar beneficiarios
        [HttpGet]
        public IActionResult CrearBeneficiario() { 
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CrearBeneficiario(Beneficiarios model)
        {
            try
            {
                using (var client = _http.CreateClient())
                {
                    var url = _conf.GetSection("Variables:UrlApi").Value + "Beneficiarios/CrearBeneficiario";

                    var userContent = JsonContent.Create(model);
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", HttpContext.Session.GetString("TokenUsuario"));

                    var response = await client.PostAsync(url, userContent);

                    if (response.IsSuccessStatusCode)
                    {
                        var respuesta = await response.Content.ReadFromJsonAsync<Respuesta>();

                        if (respuesta != null)
                        {
                            switch (respuesta.Codigo)
                            {
                                case -1: 
                                case -2: 
                                case -3: 
                                    TempData["ErrorMensaje"] = respuesta.Mensaje;
                                    return RedirectToAction("CrearBeneficiario", "Beneficiarios");

                                case 0: // Éxito
                                    TempData["SuccessMensaje"] = respuesta.Mensaje;
                                    return RedirectToAction("Index", "Beneficiarios");

                                default: // Otros errores inesperados
                                    TempData["ErrorMensaje"] = "Ocurrió un error inesperado. Código: " + respuesta.Codigo;
                                    return RedirectToAction("CrearBeneficiario", "Beneficiarios");
                            }
                        }

                        TempData["ErrorMensaje"] = "Error en la respuesta del servidor.";
                        return RedirectToAction("Crear", "Beneficiario");
                    }
                    else
                    {
                        // Manejo de errores HTTP no exitosos (4xx, 5xx)
                        var apiResponse = await response.Content.ReadFromJsonAsync<Respuesta>();
                        TempData["ErrorMensaje"] = apiResponse?.Mensaje ?? "Ocurrió un error inesperado.";
                        return RedirectToAction("Crear", "Beneficiario");
                    }
                }
            }
            catch (Exception ex)
            {
                // Manejo de excepciones
                TempData["ErrorMensaje"] = $"Error al conectar con el servidor: {ex.Message}";
                return RedirectToAction("Error", "MostrarError");
            }
        }










    }
}

