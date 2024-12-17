using fubi_client.Models;
using fubi_client.Utils.comunes;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using static System.Net.WebRequestMethods;
using System.Text.Json;

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
                    var datosUsuario = JsonSerializer.Deserialize<User>((JsonElement)result.Contenido!);

                    HttpContext.Session.SetString("id_usuario", datosUsuario!.id_usuario);
                    HttpContext.Session.SetString("Cedula", datosUsuario!.cedula.ToString());
                    HttpContext.Session.SetString("NombreUsuario", datosUsuario!.nombre);
                    HttpContext.Session.SetString("ApellidoUsuario", datosUsuario!.primer_apellido);
                    HttpContext.Session.SetString("RutaImagen", datosUsuario!.ruta_imagen);
                    HttpContext.Session.SetString("TokenUsuario", datosUsuario!.Token);
                    HttpContext.Session.SetInt32("RolUsuario", datosUsuario!.id_rol);
                    HttpContext.Session.SetString("RolNombre", datosUsuario!.rol);
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
        public IActionResult RecuperarAcceso(User Model)
        {
            return View();
        }
    }
}