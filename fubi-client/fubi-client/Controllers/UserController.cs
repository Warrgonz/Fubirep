using Microsoft.AspNetCore.Mvc;
using static System.Net.WebRequestMethods;
using fubi_client.Models;
using System.Text.Json;
using System.Reflection;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Mvc.Filters;
using System;

namespace fubi_client.Controllers
{
    public class UserController : Controller
    {
        private readonly IHttpClientFactory _http;
        private readonly IConfiguration _conf;

        public UserController(IHttpClientFactory http, IConfiguration conf)
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

        public IActionResult Index()
        {
            using (var client = _http.CreateClient())
            {
                string url = _conf.GetSection("Variables:UrlApi").Value + "User/ObtenerUsuarios";

                var auth = HttpContext.Session.GetString("TokenUsuario");

                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth);

                var response = client.GetAsync(url).Result;

                var result = response.Content.ReadFromJsonAsync<Respuesta>().Result;

                if (result != null && result.Codigo == 0 && result.Contenido != null)
                {
                    var datosContenido = JsonSerializer.Deserialize<List<User>>((JsonElement)result.Contenido);

                    return View(new List<User>(datosContenido));
                }

                return View(new List<User>());
            }
        }

        [HttpGet]
        public IActionResult CreateUser()
        {
            ConsultarRoles();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateUser(User model, IFormFile ruta_imagen)
        {
            using (var client = _http.CreateClient())
            {
                var url = _conf.GetSection("Variables:UrlApi").Value + "User/CreateUser";
                var userContent = JsonContent.Create(model);

                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", HttpContext.Session.GetString("TokenUsuario"));

                var response = await client.PostAsync(url, userContent);

                if (response.IsSuccessStatusCode)
                {
                    var imageUrl = $"{_conf.GetSection("Variables:UrlApi").Value}User/UploadUserImage/{model.cedula}";
                    var formContent = new MultipartFormDataContent();

                    if (ruta_imagen != null && ruta_imagen.Length > 0)
                    {
                        formContent.Add(new StreamContent(ruta_imagen.OpenReadStream()), "file", ruta_imagen.FileName);
                    }
                    else
                    {
                        var defaultImageUrl = _conf.GetSection("DefaultImage:imagen_defecto").Value;

                        using var httpClient = new HttpClient();
                        var imageBytes = await httpClient.GetByteArrayAsync(defaultImageUrl);

                        var stream = new MemoryStream(imageBytes);
                        formContent.Add(new StreamContent(stream), "file", "default-image.jpg");
                    }

                    var uploadResponse = await client.PostAsync(imageUrl, formContent);

                    if (uploadResponse.IsSuccessStatusCode)
                    {
                        return RedirectToAction("Index", "User");
                    }
                    else
                    {
                        ViewBag.ErrorMessage = "No se pudo cargar la imagen.";
                        return View(model);
                    }
                }
                else
                {
                    var result = await response.Content.ReadFromJsonAsync<Respuesta>();

                    if (result.Codigo == -2)
                    {
                        ViewBag.ErrorMessage = result.Mensaje;
                    }
                    else if (result.Codigo == -3)
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

        [HttpPost]
        public IActionResult ActualizarEstadoUsuario(string cedula)
        {
            try
            {
                using (var client = _http.CreateClient())
                {
                    var url = _conf.GetSection("Variables:UrlApi").Value + "User/ActualizarEstado";

                    JsonContent datos = JsonContent.Create(cedula);

                    // El hechicero
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", HttpContext.Session.GetString("TokenUsuario"));

                    var response = client.PostAsync(url, datos).Result;
                    var result = response.Content.ReadFromJsonAsync<Respuesta>().Result;


                    if (result != null && result.Codigo == 0)
                    {

                        return RedirectToAction("Index", "User");
                    }
                    else
                    {
                        TempData["ErrorMessage"] = result.Mensaje;
                        return RedirectToAction("MostrarError", "Error");
                    }
                } 
            }
            catch (Exception ex) {
                return RedirectToAction("MostrarError", "Error");
            }
        }

        [HttpGet]
        public IActionResult UpdateUser(string cedula)
        {
            ConsultarRoles();
            return View(ObtenerUsuario(cedula));
        }

        [HttpPost]
        public async Task<IActionResult> UpdateUser(User model, IFormFile ruta_imagen)
        {
            using (var client = _http.CreateClient())
            {
                var url = _conf.GetSection("Variables:UrlApi").Value + "User/UpdateUser";

                JsonContent datos = JsonContent.Create(model);
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", HttpContext.Session.GetString("TokenUsuario"));

                var response = client.PutAsync(url, datos).Result;
                var result = response.Content.ReadFromJsonAsync<Respuesta>().Result;

                if (response.IsSuccessStatusCode)
                {
                    var imageUrl = $"{_conf.GetSection("Variables:UrlApi").Value}User/UpdateUserImage/{model.cedula}";
                    var formContent = new MultipartFormDataContent();

                    if (ruta_imagen != null && ruta_imagen.Length > 0)
                    {
                        formContent.Add(new StreamContent(ruta_imagen.OpenReadStream()), "file", ruta_imagen.FileName);
                    }
                    else
                    {
                        var defaultImageUrl = _conf.GetSection("DefaultImage:imagen_defecto").Value;

                        using var httpClient = new HttpClient();
                        var imageBytes = await httpClient.GetByteArrayAsync(defaultImageUrl);

                        var stream = new MemoryStream(imageBytes);
                        formContent.Add(new StreamContent(stream), "file", "default-image.jpg");
                    }

                    var uploadResponse = await client.PostAsync(imageUrl, formContent);

                    if (uploadResponse.IsSuccessStatusCode)
                    {
                        return RedirectToAction("Index", "User");
                    }
                    else
                    {
                        ViewBag.ErrorMessage = "No se pudo cargar la imagen.";
                        return View(model);
                    }
                }
                else
                {
                    result = await response.Content.ReadFromJsonAsync<Respuesta>();

                    if (result.Codigo == -2)
                    {
                        ViewBag.ErrorMessage = result.Mensaje;
                    }
                    else if (result.Codigo == -3)
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

        // Roles

        [HttpGet]
        private void ConsultarRoles()
        {
            using (var client = _http.CreateClient())
            {
                string url = _conf.GetSection("Variables:UrlApi").Value + "User/RoleQuery";

                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", HttpContext.Session.GetString("TokenUsuario"));

                var response = client.GetAsync(url).Result;
                var result = response.Content.ReadFromJsonAsync<Respuesta>().Result;

                if (result != null && result.Codigo == 0)
                {
                    ViewBag.DropDownRoles = JsonSerializer.Deserialize<List<Role>>((JsonElement)result.Contenido!);
                }
            }
        }

        // Traer un usuario por cedula

        [HttpGet]
        private User? ObtenerUsuario(string cedula)
        {
            using (var client = _http.CreateClient())
            {
                string url = _conf.GetSection("Variables:UrlApi").Value + "User/QueryUser?cedula=" + cedula;

                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", HttpContext.Session.GetString("TokenUsuario"));

                if (string.IsNullOrEmpty(url))
                {
                    throw new Exception("La URL para la solicitud es nula o vacía.");
                }

                var response = client.GetAsync(url).Result;

                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"Error al obtener datos del usuario. Código de estado: {response.StatusCode}");
                }

                var result = response.Content.ReadFromJsonAsync<Respuesta>().Result;

                if (result != null && result.Codigo == 0 && result.Contenido != null)
                {
                    return JsonSerializer.Deserialize<User>((JsonElement)result.Contenido!);
                }

                return null; 
            }
        }



    }
}
