using Dapper;
using fubi_client.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.Tokens;
using System.Data;
using System.Linq;

namespace fubi_api.Controllers
{
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
                var result = context.Query<Beneficiarios>("ConsultarBeneficiarios", commandType: CommandType.StoredProcedure);

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
        [Route("CreateBeneficiarios")]
        public IActionResult CreateBeneficiarios([FromBody] Beneficiarios beneficiarios)
        {
            var respuesta = new Respuesta();

            try
            {
                if (!string.IsNullOrEmpty(beneficiarios.beneficiario) && !string.IsNullOrEmpty(beneficiarios.direccion) )
                {
                }
                else
                {
                    respuesta.Codigo = -3;
                    respuesta.Mensaje = "Los campos obligatorios no pueden estar vacíos.";
                    return BadRequest(respuesta);
                }

                using (var context = new SqlConnection(_conf.GetSection("ConnectionStrings:DefaultConnection").Value))
                {
                    var parameters = new
                    {
                        IdBeneficiario = beneficiarios.id_beneficiario,
                        Beneficiario= beneficiarios.beneficiario,
                        Cedula = beneficiarios.cedula,
                        Direccion = beneficiarios.direccion,
                        Telefono = beneficiarios.telefono,
                        Activo = beneficiarios.activo
                    };

                    var result = context.Execute("CreateBeneficiarios", parameters, commandType: CommandType.StoredProcedure);

                    if (result > 0)
                    {
                        respuesta.Codigo = 0;
                        respuesta.Mensaje = "Beneficiario creado correctamente.";
                    }
                    else
                    {
                        respuesta.Codigo = -1;
                        respuesta.Mensaje = "No se pudo registrar el beneficiario.";
                    }
                }
            }
            catch (SqlException ex)
            {
                if (ex.Number == 2627 || ex.Number == 2601) // Error de clave única
                {
                    respuesta.Codigo = -2;
                    respuesta.Mensaje = "El beneficiario ya está registrado en el sistema.";
                }
                else
                {
                    respuesta.Codigo = -1;
                    respuesta.Mensaje = $"Error al registrar el beneficiario: {ex.Message}";
                }
                return BadRequest(respuesta);
            }
            catch (Exception ex)
            {
                respuesta.Codigo = -1;
                respuesta.Mensaje = $"Error inesperado: {ex.Message}";
                return BadRequest(respuesta);
            }

            return Ok(respuesta);
        }

        [HttpPut]
        [Route("EditBeneficiarios")]
        public IActionResult EditBeneficiarios([FromBody] Beneficiarios beneficiarios)
        {
            var respuesta = new Respuesta();

            using (var context = new SqlConnection(_conf.GetSection("ConnectionStrings:DefaultConnection").Value))
            {
                var parameters = new
                {
                    IdBeneficiario = beneficiarios.id_beneficiario,
                    Beneficiario = beneficiarios.beneficiario,
                    Cedula = beneficiarios.cedula,
                    Direccion = beneficiarios.direccion,
                    Telefono = beneficiarios.telefono,
                    Activo = beneficiarios.activo,
                };

                var result = context.Execute("EditBeneficiarios", parameters, commandType: CommandType.StoredProcedure);

                if (result > 0)
                {
                    respuesta.Codigo = 0;
                    respuesta.Mensaje = "Beneficiario se ha editado correctamente.";
                }
                else
                {
                    respuesta.Codigo = -1;
                    respuesta.Mensaje = "No se pudo editar el beneficiario.";
                }

                return Ok(respuesta);
            }
        }

        [HttpPut]
        [Route("DeshabilitarBeneficiarios/{cedula}")]
        public IActionResult DeshabilitarBeneficiarios(string cedula)
        {
            var respuesta = new Respuesta();

            using (var context = new SqlConnection(_conf.GetSection("ConnectionStrings:DefaultConnection").Value))
            {
                var parameters = new { Cedula = cedula };
                var result = context.Execute("DeshabilitarBeneficiarios", parameters, commandType: CommandType.StoredProcedure);

                if (result > 0)
                {
                    respuesta.Codigo = 0;
                    respuesta.Mensaje = "Beneficiario deshabilitado correctamente.";
                }
                else
                {
                    respuesta.Codigo = -1;
                    respuesta.Mensaje = "No se pudo deshabilitar el beneficiario.";
                }

                return Ok(respuesta);
            }
        }
    }

    

    public class Respuesta
    {
        public int Codigo { get; set; }
        public string Mensaje { get; set; }
        public object Contenido { get; set; }
    }
}
