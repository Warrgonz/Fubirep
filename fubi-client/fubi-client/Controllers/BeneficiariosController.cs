﻿using Microsoft.AspNetCore.Mvc;
using static System.Net.WebRequestMethods;
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

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult CreateBeneficiarios()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateBeneficiarios(Beneficiarios model, IFormFile ruta_imagen)
        {
            using (var client = _http.CreateClient())
            {
                // 1. Crear beneficiario sin imagen
                var url = _conf.GetSection("Variables:UrlApi").Value + "Benefeciarios/CreateBeneficiarios";
                var userContent = JsonContent.Create(model);

                var response = await client.PostAsync(url, userContent);

                if (response.IsSuccessStatusCode)
                {
                    // 2. Subir imagen
                    if (ruta_imagen != null && ruta_imagen.Length > 0)
                    {
                        var imageUrl = $"{_conf.GetSection("Variables:UrlApi").Value}Beneficiarios/UploadBeneficiariosImage/{model.cedula}";
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
    }
}
