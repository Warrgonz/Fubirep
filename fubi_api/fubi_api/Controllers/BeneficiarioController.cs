using Dapper;
using fubi_api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;

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

        // Método para consultar todos los beneficiarios
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

        // Método para crear un beneficiario
        [HttpPost]
        [Route("CrearBeneficiario")]
        public async Task<IActionResult> CrearBeneficiario([FromBody] Beneficiarios model)
        {
            var respuesta = new Respuesta();

            try
            {
                using (var context = new SqlConnection(_conf.GetSection("ConnectionStrings:DefaultConnection").Value))
                {
                    var parameters = new
                    {
                        Cedula = model.Cedula,
                        Correo = model.Correo,
                        Telefono = model.Telefono,
                        Direccion = model.Direccion
                    };

                    var result = await context.ExecuteAsync("CrearBeneficiario", parameters, commandType: CommandType.StoredProcedure);

                    if (result > 0)
                    {
                        respuesta.Codigo = 0;
                        respuesta.Mensaje = "Beneficiario creado correctamente.";
                        return Ok(respuesta);
                    }
                    else
                    {
                        respuesta.Codigo = -1;
                        respuesta.Mensaje = "No se pudo registrar el beneficiario.";
                        return BadRequest(respuesta);
                    }
                }
            }
            catch (Exception ex)
            {
                respuesta.Codigo = -1;
                respuesta.Mensaje = $"Error al crear beneficiario: {ex.Message}";
                return BadRequest(respuesta);
            }
        }

        // Método para actualizar un beneficiario
        [HttpPut]
        [Route("ActualizarBeneficiario")]
        public async Task<IActionResult> ActualizarBeneficiario([FromBody] Beneficiarios model)
        {
            var respuesta = new Respuesta();

            try
            {
                using (var context = new SqlConnection(_conf.GetSection("ConnectionStrings:DefaultConnection").Value))
                {
                    var parameters = new
                    {
                        Id = model.Id,
                        Cedula = model.Cedula,
                        Correo = model.Correo,
                        Telefono = model.Telefono,
                        Direccion = model.Direccion
                    };

                    var result = await context.ExecuteAsync("ActualizarBeneficiario", parameters, commandType: CommandType.StoredProcedure);

                    if (result > 0)
                    {
                        respuesta.Codigo = 0;
                        respuesta.Mensaje = "Beneficiario actualizado correctamente.";
                        return Ok(respuesta);
                    }
                    else
                    {
                        respuesta.Codigo = -1;
                        respuesta.Mensaje = "No se encontró el beneficiario.";
                        return NotFound(respuesta);
                    }
                }
            }
            catch (Exception ex)
            {
                respuesta.Codigo = -1;
                respuesta.Mensaje = $"Error al actualizar beneficiario: {ex.Message}";
                return BadRequest(respuesta);
            }
        }

        // Método para eliminar un beneficiario
        [HttpDelete]
        [Route("EliminarBeneficiario/{id}")]
        public async Task<IActionResult> EliminarBeneficiario(int id)
        {
            var respuesta = new Respuesta();

            try
            {
                using (var context = new SqlConnection(_conf.GetSection("ConnectionStrings:DefaultConnection").Value))
                {
                    var result = await context.ExecuteAsync("EliminarBeneficiario", new { Id = id }, commandType: CommandType.StoredProcedure);

                    if (result > 0)
                    {
                        respuesta.Codigo = 0;
                        respuesta.Mensaje = "Beneficiario eliminado correctamente.";
                        return Ok(respuesta);
                    }
                    else
                    {
                        respuesta.Codigo = -1;
                        respuesta.Mensaje = "No se encontró el beneficiario.";
                        return NotFound(respuesta);
                    }
                }
            }
            catch (Exception ex)
            {
                respuesta.Codigo = -1;
                respuesta.Mensaje = $"Error al eliminar beneficiario: {ex.Message}";
                return BadRequest(respuesta);
            }
        }
    }
}

