using Dapper;
using fubi_api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;

[ApiController]
[Route("api/[controller]")]
public class PrestamosController : ControllerBase
{
    private readonly IConfiguration _conf;

    public PrestamosController(IConfiguration conf)
    {
        _conf = conf;
    }

    [HttpGet]
    [Route("ObtenerPrestamos")]
    public IActionResult ConsultarPrestamos()
    {
        // Conexión a la base de datos
        using (var context = new SqlConnection(_conf.GetSection("ConnectionStrings:DefaultConnection").Value))
        {
            var respuesta = new Respuesta();
            try
            {
                // Ejecución del procedimiento almacenado
                var result = context.Query<dynamic>("ObtenerPrestamos", commandType: System.Data.CommandType.StoredProcedure);

                if (result.Any())
                {
                    respuesta.Codigo = 0;
                    respuesta.Contenido = result;
                }
                else
                {
                    respuesta.Codigo = -1;
                    respuesta.Mensaje = "No hay préstamos registrados en este momento.";
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
    [Route("CrearPrestamo")]
    public async Task<IActionResult> CrearPrestamo([FromBody] PrestamoRequest prestamo)
    {
        var respuesta = new Respuesta();

        try
        {
            // Conexión a la base de datos
            using (var connection = new SqlConnection(_conf.GetConnectionString("DefaultConnection")))
            {
                var parameters = new
                {
                    IdBeneficiario = prestamo.id_beneficiario,
                    IdEncargado = prestamo.id_encargado,
                    IdInventario = prestamo.id_inventario,
                    Cantidad = prestamo.cantidad,
                    IdEstado = prestamo.id_estado,
                    FechaLimiteDevolucion = prestamo.fecha_limite_devolución
                };

                // Llamada al procedimiento almacenado
                await connection.ExecuteAsync("CrearPrestamo", parameters, commandType: System.Data.CommandType.StoredProcedure);

                respuesta.Codigo = 0;
                respuesta.Mensaje = "Préstamo creado exitosamente.";
            }

            return Ok(respuesta);
        }
        catch (SqlException ex)
        {
            respuesta.Codigo = -1;
            respuesta.Mensaje = $"Error al crear el préstamo: {ex.Message}";
            return BadRequest(respuesta);
        }
        catch (Exception ex)
        {
            respuesta.Codigo = -2;
            respuesta.Mensaje = $"Ocurrió un error inesperado: {ex.Message}";
            return StatusCode(500, respuesta);
        }
    }

    [HttpGet("ObtenerPrestamo/{id}")]
    public IActionResult ObtenerPrestamo(int id)
    {
        using (var connection = new SqlConnection(_conf.GetConnectionString("DefaultConnection")))
        {
            try
            {
                var prestamo = connection.QueryFirstOrDefault<PrestamoDetalle>(
                    "ObtenerPrestamo",
                    new { IdPrestamo = id },
                    commandType: CommandType.StoredProcedure);

                if (prestamo != null)
                {
                    return Ok(prestamo);
                }
                else
                {
                    return NotFound(new { Mensaje = "Préstamo no encontrado." });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Mensaje = $"Error al obtener el préstamo: {ex.Message}" });
            }
        }
    }
}



