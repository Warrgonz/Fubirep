using Dapper;
using fubi_api.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;
using fubi_api.Models;
using static System.Runtime.InteropServices.JavaScript.JSType;
using fubi_api.Utils.Smtp;
using System.Text;
using System.Security.Cryptography;
using fubi_api.Utils.Smtp;
using fubi_api.Utils.S3;
using Amazon.S3.Model;

namespace fubi_api.Controllers
{
    [Route("/api/[controller]")]
    [ApiController]
    public class BeneficiarioController : ControllerBase
    {
        private readonly IConfiguration _conf;
        private readonly IBucket _bucket;

        public BeneficiarioController(IConfiguration conf, IBucket bucket)
        {
            _conf = conf;
            _bucket = bucket;
        }

        // Método para crear un beneficiario
        [HttpPost]
        [Route("CreateBeneficiarios")]
        public async Task<IActionResult> CrearBeneficiarios([FromBody] Beneficiarios model)
        {
            var respuesta = new Respuesta();

            try
            {
                // Validar los campos obligatorios
                if (model.Cedula == null || string.IsNullOrEmpty(model.Beneficiario) || string.IsNullOrEmpty(model.Correo))
                {
                    respuesta.Codigo = -1;
                    respuesta.Mensaje = "Los campos obligatorios no pueden estar vacíos.";
                    return BadRequest(respuesta);
                }

                // Guardar en la base de datos sin imagen para obtener el ID
                using (var context = new SqlConnection(_conf.GetSection("ConnectionStrings:DefaultConnection").Value))
                {
                    var parameters = new
                    {
                        Cedula = model.Cedula,
                        Beneficiario = model.Beneficiario,
                        Correo = model.Correo,
                        Telefono = model.Telefono,
                        Direccion = model.Direccion,
                        RutaImagen = "" // Inicialmente no hay imagen
                    };

                    // Llamar al procedimiento almacenado
                    var result = await context.ExecuteAsync("CrearBeneficiario", parameters, commandType: CommandType.StoredProcedure);

                    if (result > 0)
                    {
                        respuesta.Codigo = 0;
                        respuesta.Mensaje = "Beneficiario creado correctamente.";
                        return Ok(new { Cedula = model.Cedula });
                    }
                    else
                    {
                        respuesta.Codigo = -1;
                        respuesta.Mensaje = "No se pudo registrar el beneficiario.";
                        return BadRequest(respuesta);
                    }
                }
            }
            catch (SqlException ex)
            {
                if (ex.Number == 2627 || ex.Number == 2601)
                {
                    respuesta.Codigo = -2;
                    respuesta.Mensaje = "La cédula ya está registrada en el sistema.";
                }
                else
                {
                    respuesta.Codigo = -1;
                    respuesta.Mensaje = $"Error al crear beneficiario: {ex.Message}";
                }
                return BadRequest(respuesta);
            }
            catch (Exception ex)
            {
                respuesta.Codigo = -1;
                respuesta.Mensaje = $"Error al crear beneficiario: {ex.Message}";
                return BadRequest(respuesta);
            }
        }

        // Método para subir la imagen del beneficiario
        [HttpPost]
        [Route("UploadBeneficiarioImage/{cedula}")]
        public async Task<IActionResult> UploadBeneficiarioImage([FromRoute] int cedula, IFormFile file)
        {
            var respuesta = new Respuesta();

            try
            {
                // Validar que el archivo no sea nulo o vacío
                if (file == null || file.Length == 0)
                {
                    respuesta.Codigo = -1;
                    respuesta.Mensaje = "El archivo no puede estar vacío.";
                    return BadRequest(respuesta);
                }

                // Subir archivo a S3 y obtener la URL
                var fileUrl = await _bucket.UploadFileAsync(file, "beneficiarios", cedula.ToString());

                // Actualizar la ruta de la imagen en la base de datos
                using (var context = new SqlConnection(_conf.GetSection("ConnectionStrings:DefaultConnection").Value))
                {
                    var parameters = new
                    {
                        Cedula = cedula,
                        RutaImagen = fileUrl
                    };

                    var updateResult = await context.ExecuteAsync("ActualizarRutaImagenBeneficiario", parameters, commandType: CommandType.StoredProcedure);

                    if (updateResult > 0)
                    {
                        respuesta.Codigo = 0;
                        respuesta.Mensaje = "Imagen actualizada correctamente.";
                    }
                    else
                    {
                        respuesta.Codigo = -1;
                        respuesta.Mensaje = "No se pudo actualizar la imagen del beneficiario.";
                    }
                }
            }
            catch (Exception ex)
            {
                respuesta.Codigo = -1;
                respuesta.Mensaje = $"Error al subir la imagen: {ex.Message}";
            }

            return Ok(respuesta);
        }
    }
}

