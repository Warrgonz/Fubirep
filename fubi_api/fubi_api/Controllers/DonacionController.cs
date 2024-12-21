using Dapper;
using fubi_api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;

namespace fubi_api.Controllers
{
    [Route("/api/[controller]")]
    [ApiController]
    public class DonacionController : ControllerBase
    {
        private readonly IConfiguration _conf;

        public DonacionController(IConfiguration conf)
        {
            _conf = conf;
        }

        [HttpGet]
        [Route("ObtenerDonaciones")]
        public IActionResult ConsultarDonaciones()
        {
            using (var context = new SqlConnection(_conf.GetConnectionString("DefaultConnection")))
            {
                var respuesta = new Respuesta();
                try
                {
                    var result = context.Query<dynamic>("ConsultarDonaciones", commandType: CommandType.StoredProcedure).ToList();

                    if (result.Any())
                    {
                        respuesta.Codigo = 0;
                        respuesta.Contenido = result; // Devuelve la lista de donaciones con relaciones
                    }
                    else
                    {
                        respuesta.Codigo = -1;
                        respuesta.Mensaje = "No hay donaciones registradas en este momento.";
                    }
                }
                catch (Exception ex)
                {
                    respuesta.Codigo = -2;
                    respuesta.Mensaje = $"Ocurrió un error: {ex.Message}";
                }

                return Ok(respuesta);
            }
        }



        [HttpPost]
        [Route("CreateDonacion")]
        public async Task<IActionResult> CrearDonacion([FromBody] Donacion model)
        {
            var respuesta = new Respuesta();
            try
            {
                using (var context = new SqlConnection(_conf.GetSection("ConnectionStrings:DefaultConnection").Value))
                {
                    var parameters = new
                    {
                        model.IdTipoMovimiento,
                        model.IdTipoDonacion,
                        model.Donante,
                        model.IdBeneficiario,
                        model.Monto,
                        model.IdInventario,
                        model.Cantidad,
                        model.Fecha
                    };

                    var result = await context.ExecuteAsync("CrearDonacion", parameters, commandType: CommandType.StoredProcedure);

                    if (result > 0)
                    {
                        respuesta.Codigo = 0;
                        respuesta.Mensaje = "Donación creada correctamente.";
                        return Ok(respuesta);
                    }
                    else
                    {
                        respuesta.Codigo = -1;
                        respuesta.Mensaje = "Error al registrar la donación.";
                        return BadRequest(respuesta);
                    }
                }
            }
            catch (Exception ex)
            {
                respuesta.Codigo = -1;
                respuesta.Mensaje = $"Error: {ex.Message}";
                return BadRequest(respuesta);
            }
        }

        [HttpGet]
        [Route("ObtenerTiposMovimiento")]
        public IActionResult ObtenerTiposMovimiento()
        {
            using (var context = new SqlConnection(_conf.GetConnectionString("DefaultConnection")))
            {
                var respuesta = new Respuesta();
                try
                {
                    var result = context.Query<dynamic>("ObtenerTiposMovimiento", commandType: CommandType.StoredProcedure);

                    if (result.Any())
                    {
                        respuesta.Codigo = 0;
                        respuesta.Contenido = result;
                    }
                    else
                    {
                        respuesta.Codigo = -1;
                        respuesta.Mensaje = "No se encontraron tipos de movimiento.";
                    }
                }
                catch (Exception ex)
                {
                    respuesta.Codigo = -2;
                    respuesta.Mensaje = $"Ocurrió un error: {ex.Message}";
                }

                return Ok(respuesta);
            }
        }

        [HttpGet]
        [Route("ObtenerTiposDonacion")]
        public IActionResult ObtenerTiposDonacion()
        {
            using (var context = new SqlConnection(_conf.GetConnectionString("DefaultConnection")))
            {
                var respuesta = new Respuesta();
                try
                {
                    var result = context.Query<dynamic>("ObtenerTiposDonacion", commandType: CommandType.StoredProcedure);

                    if (result.Any())
                    {
                        respuesta.Codigo = 0;
                        respuesta.Contenido = result;
                    }
                    else
                    {
                        respuesta.Codigo = -1;
                        respuesta.Mensaje = "No se encontraron tipos de donación.";
                    }
                }
                catch (Exception ex)
                {
                    respuesta.Codigo = -2;
                    respuesta.Mensaje = $"Ocurrió un error: {ex.Message}";
                }

                return Ok(respuesta);
            }
        }

        [HttpGet]
        [Route("ObtenerBeneficiarios")]
        public IActionResult ObtenerBeneficiarios()
        {
            using (var context = new SqlConnection(_conf.GetConnectionString("DefaultConnection")))
            {
                var respuesta = new Respuesta();
                try
                {
                    var result = context.Query<dynamic>("ObtenerBeneficiarios", commandType: CommandType.StoredProcedure);

                    if (result.Any())
                    {
                        respuesta.Codigo = 0;
                        respuesta.Contenido = result;
                    }
                    else
                    {
                        respuesta.Codigo = -1;
                        respuesta.Mensaje = "No se encontraron beneficiarios.";
                    }
                }
                catch (Exception ex)
                {
                    respuesta.Codigo = -2;
                    respuesta.Mensaje = $"Ocurrió un error: {ex.Message}";
                }

                return Ok(respuesta);
            }
        }


        [HttpGet]
        [Route("ObtenerInventarios")]
        public IActionResult ObtenerInventarios()
        {
            using (var context = new SqlConnection(_conf.GetConnectionString("DefaultConnection")))
            {
                var respuesta = new Respuesta();
                try
                {
                    var result = context.Query<dynamic>("ObtenerInventarios", commandType: CommandType.StoredProcedure);

                    if (result.Any())
                    {
                        respuesta.Codigo = 0;
                        respuesta.Contenido = result;
                    }
                    else
                    {
                        respuesta.Codigo = -1;
                        respuesta.Mensaje = "No se encontraron inventarios.";
                    }
                }
                catch (Exception ex)
                {
                    respuesta.Codigo = -2;
                    respuesta.Mensaje = $"Ocurrió un error: {ex.Message}";
                }

                return Ok(respuesta);
            }
        }

        [HttpDelete]
        [Route("EliminarDonacion/{id}")]
        public async Task<IActionResult> EliminarDonacion(int id)
        {
            using (var context = new SqlConnection(_conf.GetSection("ConnectionStrings:DefaultConnection").Value))
            {
                var respuesta = new Respuesta();

                try
                {
                    var result = await context.ExecuteAsync("EliminarDonacion", new { IdDonacion = id }, commandType: CommandType.StoredProcedure);

                    if (result > 0)
                    {
                        respuesta.Codigo = 0;
                        respuesta.Mensaje = "Donación eliminada correctamente.";
                        return Ok(respuesta);
                    }
                    else
                    {
                        respuesta.Codigo = -1;
                        respuesta.Mensaje = "No se encontró la donación.";
                        return NotFound(respuesta);
                    }
                }
                catch (Exception ex)
                {
                    respuesta.Codigo = -1;
                    respuesta.Mensaje = $"Error al eliminar donación: {ex.Message}";
                    return BadRequest(respuesta);
                }
            }
        }



    }
}
