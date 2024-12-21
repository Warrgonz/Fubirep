using Dapper;
using fubi_api.Models;
using fubi_api.Utils.Smtp;
using fubi_client.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.Tokens;
using System.Data;
using System.Linq;

namespace fubi_api.Controllers
{
    [Authorize]
    [Route("/api/[controller]")]
    [ApiController]
    public class BeneficiariosController : ControllerBase
    {
        private readonly IConfiguration _conf;

        public BeneficiariosController(IConfiguration conf)
        {
            _conf = conf;
        }



        [HttpGet]
        [Route("ObtenerBeneficiarios")]
        public IActionResult ConsultarBeneficiarios()
        {
            using (var context = new SqlConnection(_conf.GetSection("ConnectionStrings:DefaultConnection").Value))
            {
                var respuesta = new Respuesta();

                // Ejecuta el stored procedure ConsultarBeneficiarios
                var result = context.Query<Beneficiarios>("ConsultarBeneficiarios", commandType: CommandType.StoredProcedure).ToList();

                if (result.Any())
                {
                    respuesta.Codigo = 0;
                    respuesta.Contenido = result;
                }
                else
                {
                    respuesta.Codigo = -1;
                    respuesta.Mensaje = "No hay beneficiarios registrados en este momento.";
                }

                return Ok(respuesta);
            }
        }

        [HttpGet]
        [Route("ObtenerPrestBeneficiarios")]
        public IActionResult ConsultarPrestBeneficiarios()
        {
            using (var context = new SqlConnection(_conf.GetSection("ConnectionStrings:DefaultConnection").Value))
            {
                var respuesta = new Respuesta();

                // Ejecuta el stored procedure ConsultarBeneficiarios
                var result = context.Query<Beneficiarios>("ConsultarPrestBeneficiarios", commandType: CommandType.StoredProcedure).ToList();

                if (result.Any())
                {
                    respuesta.Codigo = 0;
                    respuesta.Contenido = result;
                }
                else
                {
                    respuesta.Codigo = -1;
                    respuesta.Mensaje = "No hay beneficiarios registrados en este momento.";
                }

                return Ok(respuesta);
            }
        }

        [HttpPost]
        [Route("CrearBeneficiario")]
        public async Task<IActionResult> CrearBeneficiario([FromBody] Beneficiarios model)
        {
            var respuesta = new Respuesta();

            if (string.IsNullOrWhiteSpace(model.Beneficiario) ||
    string.IsNullOrWhiteSpace(model.Cedula) ||
    string.IsNullOrWhiteSpace(model.Correo))
            {
                respuesta.Codigo = -1;
               respuesta.Mensaje = "El campo requeridos no pueden estar vacío.";
               return Ok(respuesta);
            }

            if (await CedulaExiste(model.Cedula))
            {
                respuesta.Codigo = -2;
                respuesta.Mensaje = "El beneficiario con la cédula ingresada, ya existe.";
                return Ok(respuesta);
            }

            if (await CorreoExiste(model.Correo))
            {
                respuesta.Codigo = -3;
                respuesta.Mensaje = "El beneficiario con el correo ingresado, ya existe.";
                return Ok(respuesta);
            }

            using (var context = new SqlConnection(_conf.GetSection("ConnectionStrings:DefaultConnection").Value))
            {
                var parameters = new
                {
                    Cedula = model.Cedula,
                    Beneficiario = model.Beneficiario,
                    Correo = model.Correo,
                    Telefono = model.Telefono,
                    Direccion = model.Direccion
                };

                var rowsAffected = await context.ExecuteAsync(
                    "CrearBeneficiario",
                    parameters,
                    commandType: CommandType.StoredProcedure
                );

                if (rowsAffected > 0)
                {
                    return Ok(new Respuesta { Codigo = 0, Mensaje = "Beneficiario creado exitosamente." });
                }
                else
                {
                    return StatusCode(500, new Respuesta { Codigo = -4, Mensaje = "Error al crear beneficiario." });
                }
            }
        }

        private async Task<bool> CedulaExiste(string cedula)
        {
            using (var context = new SqlConnection(_conf.GetSection("ConnectionStrings:DefaultConnection").Value))
            {
                var parameters = new { Cedula = cedula };
                var existe = await context.ExecuteScalarAsync<int>(
                    "ConsultarCedulaBeneficiario",
                    parameters,
                    commandType: CommandType.StoredProcedure
                );

                return existe > 0;
            }
        }

        private async Task<bool> CorreoExiste(string correo)
        {
            using (var context = new SqlConnection(_conf.GetSection("ConnectionStrings:DefaultConnection").Value))
            {
                var parameters = new { Correo = correo };
                var existe = await context.ExecuteScalarAsync<int>(
                    "ConsultarCorreoBeneficiario",
                    parameters,
                    commandType: CommandType.StoredProcedure
                );

                return existe > 0;
            }
        }



    }

}


