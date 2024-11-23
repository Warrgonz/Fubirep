using Dapper;
using fubi_api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;

namespace fubi_api.Controllers
{
    [Route("/api/[controller]")]
    [ApiController]
    public class InventarioController : ControllerBase
    {
        private readonly IConfiguration _conf;

        public InventarioController(IConfiguration conf)
        {
            _conf = conf;
        }

        [HttpGet]
        [Route("ObtenerInventarios")]
        public IActionResult ConsultarInventarios()
        {
            using (var context = new SqlConnection(_conf.GetSection("ConnectionStrings:DefaultConnection").Value))
            {
                var respuesta = new Respuesta();
                var result = context.Query<Inventario>("ConsultarInventario", new { });

                if (result.Any())
                {
                    respuesta.Codigo = 0;
                    respuesta.Contenido = result;
                }
                else
                {
                    respuesta.Codigo = -1;
                    respuesta.Mensaje = "No hay inventarios registrados en este momento.";
                }

                return Ok(respuesta);
            }
        }



        // Método para crear el inventario
        [HttpPost]
        [Route("CreateInventario")]
        public async Task<IActionResult> CrearInventario([FromBody] Inventario model)
        {
            var respuesta = new Respuesta();

            try
            {
                // Guardar en la base de datos sin imagen
                using (var context = new SqlConnection(_conf.GetSection("ConnectionStrings:DefaultConnection").Value))
                {
                    var parameters = new
                    {
                        Nombre = model.nombre,
                        Descripcion = model.descripcion,
                        Cantidad = model.cantidad,
                        CantidadPrestada = model.cantidad_prestada,
                        RutaImagen = ""  //imagen vacia, cambiar luego
                    };

                    // Aquí ejecutamos el procedimiento almacenado que crea el inventario
                    var result = await context.ExecuteAsync("CrearInventario", parameters, commandType: CommandType.StoredProcedure);


                    if (result > 0)
                    {
                        respuesta.Codigo = 0;
                        respuesta.Mensaje = "Inventario creado correctamente.";

                        return Ok(new { IdInventario = model.id_inventario });
                    }
                    else
                    {
                        respuesta.Codigo = -1;
                        respuesta.Mensaje = "No se pudo registrar el inventario.";
                        return BadRequest(respuesta);
                    }
                }
            }
            catch (Exception ex)
            {
                respuesta.Codigo = -1;
                respuesta.Mensaje = $"Error al crear inventario: {ex.Message}";
                return BadRequest(respuesta);
            }
        }

        [HttpDelete]
        [Route("EliminarInventario/{id}")]
        public async Task<IActionResult> EliminarInventario(int id)
        {
            using (var context = new SqlConnection(_conf.GetSection("ConnectionStrings:DefaultConnection").Value))
            {
                var respuesta = new Respuesta();

                try
                {
                    var result = await context.ExecuteAsync("EliminarInventario", new { IdInventario = id }, commandType: CommandType.StoredProcedure);

                    if (result > 0)
                    {
                        respuesta.Codigo = 0;
                        respuesta.Mensaje = "Inventario eliminado correctamente.";
                        return Ok(respuesta);
                    }
                    else
                    {
                        respuesta.Codigo = -1;
                        respuesta.Mensaje = "No se encontró el inventario.";
                        return NotFound(respuesta);
                    }
                }
                catch (Exception ex)
                {
                    respuesta.Codigo = -1;
                    respuesta.Mensaje = $"Error al eliminar inventario: {ex.Message}";
                    return BadRequest(respuesta);
                }
            }
        }
        [HttpPut]
        [Route("ActualizarInventario")]
        public async Task<IActionResult> ActualizarInventario([FromBody] Inventario model)
        {
            using (var context = new SqlConnection(_conf.GetSection("ConnectionStrings:DefaultConnection").Value))
            {
                var respuesta = new Respuesta();

                try
                {
                    var parameters = new
                    {
                        IdInventario = model.id_inventario,
                        Nombre = model.nombre,
                        Descripcion = model.descripcion,
                        Cantidad = model.cantidad,
                        CantidadPrestada = model.cantidad_prestada,
                        RutaImagen = model.ruta_imagen
                    };

                    var result = await context.ExecuteAsync("ActualizarInventario", parameters, commandType: CommandType.StoredProcedure);

                    if (result > 0)
                    {
                        respuesta.Codigo = 0;
                        respuesta.Mensaje = "Inventario actualizado correctamente.";
                        return Ok(respuesta);
                    }
                    else
                    {
                        respuesta.Codigo = -1;
                        respuesta.Mensaje = "No se encontró el inventario.";
                        return NotFound(respuesta);
                    }
                }
                catch (Exception ex)
                {
                    respuesta.Codigo = -1;
                    respuesta.Mensaje = $"Error al actualizar inventario: {ex.Message}";
                    return BadRequest(respuesta);
                }
            }
        }
    }
}