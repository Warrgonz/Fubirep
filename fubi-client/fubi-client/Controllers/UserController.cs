using Microsoft.AspNetCore.Mvc;
using static System.Net.WebRequestMethods;
using fubi_client.Models;
using System.Text.Json;
using System.Reflection;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Mvc.Filters;

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
                // La URL de la API donde se obtiene la lista de usuarios
                string url = _conf.GetSection("Variables:UrlApi").Value + "User/ObtenerUsuarios";

                // Realizamos la solicitud GET al endpoint de la API
                var response = client.GetAsync(url).Result;

                // Leemos la respuesta como un objeto de tipo 'Respuesta'
                var result = response.Content.ReadFromJsonAsync<Respuesta>().Result;

                // Si la respuesta es exitosa y tiene datos
                if (result != null && result.Codigo == 0 && result.Contenido != null)
                {
                    // Deserializamos el contenido en una lista de usuarios
                    var datosContenido = JsonSerializer.Deserialize<List<User>>((JsonElement)result.Contenido);

                    // Devolvemos la vista con los usuarios obtenidos
                    return View(new List<User>(datosContenido));
                }

                // Si no hay usuarios o la respuesta fue incorrecta, devolvemos una lista vacía
                return View(new List<User>());
            }
        }

        [HttpGet]
        public IActionResult CreateUser()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateUser(User model, IFormFile ruta_imagen)
        {
            using (var client = _http.CreateClient())
            {
                var url = _conf.GetSection("Variables:UrlApi").Value + "User/CreateUser";
                var userContent = JsonContent.Create(model);

                var response = await client.PostAsync(url, userContent);

                if (response.IsSuccessStatusCode)
                {
                    var imageUrl = $"{_conf.GetSection("Variables:UrlApi").Value}User/UploadUserImage/{model.cedula}";
                    var formContent = new MultipartFormDataContent();

                    if (ruta_imagen != null && ruta_imagen.Length > 0)
                    {
                        // Subir imagen proporcionada por el usuario
                        formContent.Add(new StreamContent(ruta_imagen.OpenReadStream()), "file", ruta_imagen.FileName);
                    }
                    else
                    {
                        // Asignar imagen por defecto
                        var defaultImageUrl = _conf.GetSection("DefaultImage:imagen_defecto").Value;

                        // Descargar la imagen por defecto
                        using var httpClient = new HttpClient();
                        var imageBytes = await httpClient.GetByteArrayAsync(defaultImageUrl);

                        // Crear un flujo y asignarlo al contenido
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
        public IActionResult DesactivarUsuario()
        {
            return View();
        }

        // Actualizar usuarios

        [HttpGet]
        public IActionResult UpdateUser(string cedula)
        {
            var ced = HttpContext.Session.GetString("Cedula");
            ConsultarRoles();
            return View(ObtenerUsuario(cedula));
        }

        [HttpPost]
        public IActionResult UpdateUser(User model)
        {
            using (var client = _http.CreateClient())
            {
                var url = _conf.GetSection("Variables:UrlApi").Value + "User/UpdateUser";

                JsonContent datos = JsonContent.Create(model);

                var response = client.PutAsync(url, datos).Result;
                var result = response.Content.ReadFromJsonAsync<Respuesta>().Result;

                if (result != null && result.Codigo == 0)
                {
                    ViewBag.Mensaje = result!.Mensaje;
                    return RedirectToAction("Index", "User");
                }
                else
                {
                    ConsultarRoles();
                    ViewBag.Mensaje = result!.Mensaje;
                    return View();
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

                var response = client.GetAsync(url).Result;
                var result = response.Content.ReadFromJsonAsync<Respuesta>().Result;

                if (result != null && result.Codigo == 0)
                {
                    ViewBag.DropDownRoles = JsonSerializer.Deserialize<List<Role>>((JsonElement)result.Contenido!);
                }
            }
        }

        [HttpGet]
        public IActionResult MiPerfil()
        {
            // Obtener la cédula del usuario logueado desde la sesión
            var cedula = HttpContext.Session.GetString("CedulaUsuario");

            if (string.IsNullOrEmpty(cedula))
            {
                return RedirectToAction("Login", "Auth"); // Redirigir si no está logueado
            }

            // Obtener los datos del usuario desde la API
            var usuario = ObtenerUsuario(cedula);

            if (usuario == null)
            {
                ViewBag.ErrorMessage = "No se pudieron cargar los datos del perfil.";
                return View();
            }

            return View(usuario); // Enviar el modelo a la vista
        }

        [HttpPost]
        public IActionResult MiPerfil(User model)
        {
            if (!ModelState.IsValid)
            {
                return View(model); // Retornar errores de validación
            }

            // Obtener la cédula del usuario logueado desde la sesión
            var cedula = HttpContext.Session.GetString("CedulaUsuario");

            if (string.IsNullOrEmpty(cedula))
            {
                return RedirectToAction("Login", "Auth"); // Redirigir si no está logueado
            }

            model.cedula = cedula; // Asegurarse de no cambiar la cédula del usuario

            using (var client = _http.CreateClient())
            {
                var url = _conf.GetSection("Variables:UrlApi").Value + "User/UpdateUser";
                var datos = JsonContent.Create(model);

                var response = client.PutAsync(url, datos).Result;
                var result = response.Content.ReadFromJsonAsync<Respuesta>().Result;

                if (result != null && result.Codigo == 0)
                {
                    ViewBag.SuccessMessage = "Perfil actualizado con éxito.";
                    return RedirectToAction("MiPerfil");
                }

                ViewBag.ErrorMessage = result?.Mensaje ?? "Hubo un error al actualizar el perfil.";
                return View(model);
            }
        }

        // Traer un usuario por cedula

        [HttpGet]
        private User? ObtenerUsuario(string cedula)
        {
            using (var client = _http.CreateClient())
            {
                string url = _conf.GetSection("Variables:UrlApi").Value + "User/QueryUser?cedula=" + cedula;

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
