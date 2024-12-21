using fubi_client.Models;
using fubi_client.Utils.comunes;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using static System.Net.WebRequestMethods;
using System.Text.Json;
using System.Reflection;
using System.Net;
using System.Text.RegularExpressions;

namespace fubi_client.Controllers
{
    public class HomeController : Controller
    {
        private readonly IHttpClientFactory _http;
        private readonly IConfiguration _conf;
        private readonly IComunes _comunes;

        public HomeController(IHttpClientFactory http, IConfiguration conf, IComunes comunes)
        {
            _http = http;
            _conf = conf;
            _comunes = comunes;
        }

        public IActionResult Index()
        {
            if (HttpContext.Session.Get("NombreUsuario") != null)
            {
                return RedirectToAction("Index", "Dashboard");
            }
            if (TempData["ErrorMessage"] != null)
            {
                ViewBag.Mensaje = TempData["ErrorMessage"];
            }
            return View();
        }


        [HttpPost]
        public IActionResult IniciarSesion(User model)
        {
            using (var client = _http.CreateClient())
            {
                var url = _conf.GetSection("Variables:UrlApi").Value + "Auth/IniciarSesion";

                model.contrasena = _comunes.Hashear(model.contrasena);
                JsonContent datos = JsonContent.Create(model);
                
                var response = client.PostAsync(url, datos).Result;
                var result = response.Content.ReadFromJsonAsync<Respuesta>().Result;


                if (result != null && result.Codigo == 0)
                {
                    var json = JsonSerializer.Serialize(result.Contenido);
                    var datosUsuario = JsonSerializer.Deserialize<User>(json);
                    datosUsuario.Token = JsonSerializer.Deserialize<JsonElement>(json).GetProperty("token").GetString();

                    HttpContext.Session.SetString("id_usuario", datosUsuario.id_usuario);
                    HttpContext.Session.SetString("Cedula", datosUsuario.cedula.ToString());
                    HttpContext.Session.SetString("NombreUsuario", datosUsuario.nombre);
                    HttpContext.Session.SetString("ApellidoUsuario", datosUsuario.primer_apellido);
                    HttpContext.Session.SetString("RutaImagen", datosUsuario.ruta_imagen);
                    HttpContext.Session.SetString("TokenUsuario", datosUsuario.Token);
                    HttpContext.Session.SetInt32("RolUsuario", datosUsuario.id_rol);
                    HttpContext.Session.SetString("RolNombre", datosUsuario.rol);
                    return RedirectToAction("Index", "Dashboard");
                }
                else
                {
                    TempData["ErrorMessage"] = result.Mensaje;
                    return RedirectToAction("Index", "Home");
                }
            }
        }

        [HttpGet]
        public IActionResult CerrarSesion()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult RecuperarAcceso()
        {
            return View();
        }


        [HttpPost]
        public async Task<IActionResult> RecuperarAcceso(User model)
        {
            using (var client = _http.CreateClient())
            {
                var url = _conf.GetSection("Variables:UrlApi").Value + "Auth/RecuperarAcceso";
                var response = await client.PostAsJsonAsync(url, model);
                var result = await response.Content.ReadFromJsonAsync<Respuesta>();

                if (result != null && result.Codigo == 0)
                {
                    TempData["SuccessMessage"] = "Revisa tu correo electrónico para continuar con la recuperación de contraseña.";
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    TempData["ErrorMessage"] = result?.Mensaje ?? "Error inesperado. Por favor intenta nuevamente.";
                    return RedirectToAction("Index", "Home");
                }
            }
        }

        [HttpGet]
        public IActionResult RestablecerContrasena(string token)
        {
            try
            {
                using (var client = _http.CreateClient())
                {
                    string url = _conf.GetSection("Variables:UrlApi").Value + $"Auth/ObtenerUsuarioPorToken?token={token}";
                    var response = client.GetAsync(url).Result;

                    if (response.IsSuccessStatusCode)
                    {
                        var result = response.Content.ReadFromJsonAsync<Respuesta>().Result;
                        if (result != null && result.Codigo == 0)
                        {
                            ViewData["Token"] = token;
                            var jsonUsuario = JsonSerializer.Serialize(result.Contenido);
                            var usuario = JsonSerializer.Deserialize<User>(jsonUsuario);

                            return View(usuario);
                        }
                        else
                        {
                            ViewBag.ErrorMessage = result?.Mensaje ?? "No se encontró el usuario.";
                            return RedirectToAction("NotFound", "Error");
                        }
                    }
                    else if (response.StatusCode == HttpStatusCode.Gone)  
                    {
                        TempData["WarningMessage"] = "El token ha expirado. Por favor solicite un nuevo enlace.";
                        return RedirectToAction("RecuperarAcceso", "Home");  
                    }
                    else
                    {
                        return RedirectToAction("NotFound", "Error");
                    }
                }
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = $"Error inesperado: {ex.Message}";
                return View();
            }
        }


        [HttpPost]
        public async Task<IActionResult> RestablecerContrasena(User model, string token)
        {
            try
            {
                // Validar si las contraseñas coinciden
                if (model.contrasena != model.contrasenaConfirmar)
                {
                    TempData["ErrorMessage"] = "Las contraseñas no coinciden. Por favor, inténtalo nuevamente.";
                    return View(model);
                }

                // Validar la complejidad de la contraseña
                var passwordRegex = new Regex(@"^(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{6,}$");
                if (!passwordRegex.IsMatch(model.contrasena))
                {
                    TempData["ErrorMessage"] = "La contraseña debe tener al menos 6 caracteres, incluir una letra mayúscula, un número y un carácter especial.";
                    return View(model);
                }

                model.contrasena = _comunes.Hashear(model.contrasena);

                using (var client = _http.CreateClient())
                {
                    string url = _conf.GetSection("Variables:UrlApi").Value + "Auth/ActualizarContrasena";
                    var response = await client.PostAsJsonAsync(url, model);

                    if (response.IsSuccessStatusCode)
                    {
                        TempData["SuccessMessage"] = "Tu contraseña se ha actualizado correctamente.";
                        return RedirectToAction("Index", "Home");
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Ocurrió un error al intentar actualizar tu contraseña. Por favor, inténtalo más tarde.";
                        return View(model);
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error inesperado: {ex.Message}";
                return View(model);
            }
        }











    }
}