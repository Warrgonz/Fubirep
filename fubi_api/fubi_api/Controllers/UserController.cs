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


//using Firebase.Auth;

namespace fubi_api.Controllers
{
    [Route("/api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IConfiguration _conf;
        private readonly IBucket _bucket;

        public UserController(IConfiguration conf, IBucket bucket)
        {
            _conf = conf;
            _bucket = bucket;
        }

        [HttpGet]
        [Route("ObtenerUsuarios")]
        public IActionResult ConsultarUsuarios()
        {
            using (var context = new SqlConnection(_conf.GetSection("ConnectionStrings:DefaultConnection").Value))
            {
                var respuesta = new Respuesta();
                var result = context.Query<User>("ConsultarUsuarios", new { });

                if (result.Any())
                {
                    respuesta.Codigo = 0;
                    respuesta.Contenido = result;
                }
                else
                {
                    respuesta.Codigo = -1;
                    respuesta.Mensaje = "No hay usuarios registrados en este momento";
                }

                return Ok(respuesta);
            }
        }

        // Método para crear el usuario y subir la imagen
        [HttpPost]
        [Route("CreateUser")]
        public async Task<IActionResult> CrearCuenta([FromBody] User model, [FromServices] IMessage emailService)
        {
            var respuesta = new Respuesta();

            try
            {
                // Validar los campos obligatorios
                if (string.IsNullOrEmpty(model.cedula) || string.IsNullOrEmpty(model.primer_apellido) || string.IsNullOrEmpty(model.correo) || string.IsNullOrEmpty(model.nombre))
                {
                    respuesta.Codigo = -3;
                    respuesta.Mensaje = "Los campos obligatorios no pueden estar vacíos.";
                    return BadRequest(respuesta);
                }

                // Generar contraseña aleatoria
                var randomPassword = GenerateRandomPassword(8);
                var encryptedPassword = EncryptPassword(randomPassword);

                // Guardar en la base de datos sin imagen, para obtener el userId
                using (var context = new SqlConnection(_conf.GetSection("ConnectionStrings:DefaultConnection").Value))
                {
                    var parameters = new
                    {
                        Cedula = model.cedula,
                        Nombre = model.nombre,
                        PrimerApellido = model.primer_apellido,
                        SegundoApellido = model.segundo_apellido,
                        Correo = model.correo,
                        Contrasena = encryptedPassword,
                        Telefono = model.telefono,
                        FechaNacimiento = model.fecha_nacimiento,
                        RutaImagen = "",  
                        Rol = model.rol
                    };

                    // Aquí ejecutamos el procedimiento almacenado que crea el usuario
                    var result = await context.ExecuteAsync("CrearCuenta", parameters, commandType: CommandType.StoredProcedure);

                    if (result > 0)
                    {
                        // Enviar correo de bienvenida
                        emailService.SendEmail(
                            "¡Bienvenido a Fubiredip!",
                            $"Hola {model.nombre},<br><br>Tu cuenta ha sido creada correctamente.<br>Tu correo es: <b>{model.correo}</b><br>Tu contraseña es: <b>{randomPassword}</b><br><br>Gracias por registrarte.",
                            model.correo
                        );

                        respuesta.Codigo = 0;
                        respuesta.Mensaje = "Usuario creado correctamente. Se ha enviado un correo con la contraseña.";

                        // Devolver la cedula (o cualquier otro dato identificador como el userId si es necesario)
                        return Ok(new { Cedula = model.cedula });
                    }
                    else
                    {
                        respuesta.Codigo = -1;
                        respuesta.Mensaje = "No se pudo registrar el usuario.";
                        return BadRequest(respuesta);
                    }
                }
            }
            catch (SqlException ex)
            {
                // Capturamos el error relacionado con la violación de restricción de unicidad
                if (ex.Number == 2627 || ex.Number == 2601)
                {
                    // Error de violación de clave única
                    respuesta.Codigo = -2;
                    respuesta.Mensaje = "El usuario ya está registrado en el sistema.";
                }
                else
                {
                    // Otros errores
                    respuesta.Codigo = -1;
                    respuesta.Mensaje = $"Error al crear usuario: {ex.Message}";
                }
                return BadRequest(respuesta);
            }
            catch (Exception ex)
            {
                respuesta.Codigo = -1;
                respuesta.Mensaje = $"Error al crear usuario: {ex.Message}";
                return BadRequest(respuesta);
            }
        }

        [HttpPost]
        [Route("UploadUserImage/{cedula}")]
        public async Task<IActionResult> UploadUserImage([FromRoute] string cedula, IFormFile file)
        {
            var respuesta = new Respuesta();

            try
            {

                // Validar que el archivo no sea nulo o vacío
                if (file == null || file.Length == 0)
                {
                    var defaultImageUrl = _conf.GetSection("DefaultImage:imagen_defecto").Value;

                    // Descargar la imagen desde la URL
                    using var httpClient = new HttpClient();
                    var imageBytes = await httpClient.GetByteArrayAsync(defaultImageUrl);

                    // Convertir los bytes a un flujo
                    var stream = new MemoryStream(imageBytes);

                    // Crear un IFormFile usando el flujo
                    file = new FormFile(stream, 0, stream.Length, "file", "default-image.jpg");
                }

                // Subir archivo a S3 y obtener la URL
                var fileUrl = await _bucket.UploadFileAsync(file, "avatar", cedula);

                // Actualizar la ruta de la imagen en la base de datos
                using (var context = new SqlConnection(_conf.GetSection("ConnectionStrings:DefaultConnection").Value))
                {
                    var parameters = new
                    {
                        Cedula = cedula,
                        RutaImagen = fileUrl
                    };

                    var updateResult = await context.ExecuteAsync("ActualizarRutaImagen", parameters, commandType: CommandType.StoredProcedure);

                    if (updateResult > 0)
                    {
                        respuesta.Codigo = 0;
                        respuesta.Mensaje = "Imagen actualizada correctamente.";
                    }
                    else
                    {
                        respuesta.Codigo = -1;
                        respuesta.Mensaje = "No se pudo actualizar la imagen del usuario.";
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

        [HttpPost]
        [Route("DesactivarUsuario/{cedula}")]
        public IActionResult DesactivarUsuario(string cedula)
        {
            Console.WriteLine($"Intentando desactivar usuario con cédula: {cedula}");
            var respuesta = new Respuesta();

            try
            {
                using (var context = new SqlConnection(_conf.GetSection("ConnectionStrings:DefaultConnection").Value))
                {
                    Console.WriteLine("Conexión a base de datos establecida.");

                    var parameters = new { Cedula = cedula };

                    Console.WriteLine("Ejecutando procedimiento almacenado...");

                    var rowsAffected = context.QuerySingleOrDefault<int>(
                        "DesactivarUsuario",
                        parameters,
                        commandType: CommandType.StoredProcedure
                    );

                    Console.WriteLine($"Filas afectadas: {rowsAffected}");

                    if (rowsAffected > 0)
                    {
                        respuesta.Codigo = 0;
                        respuesta.Mensaje = "Usuario desactivado exitosamente.";
                    }
                    else
                    {
                        respuesta.Codigo = -1;
                        respuesta.Mensaje = "No se encontró el usuario o ya estaba desactivado.";
                    }
                }
            }
            catch (SqlException sqlEx)
            {
                Console.WriteLine($"Error de SQL: {sqlEx.Message}");
                respuesta.Codigo = -2;
                respuesta.Mensaje = "Error al comunicarse con la base de datos.";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error inesperado: {ex.Message}");
                respuesta.Codigo = -3;
                respuesta.Mensaje = $"Error inesperado: {ex.Message}";
            }

            // Devuelve la respuesta como JSON usando Ok()
            return Ok(respuesta);
        }




        private string GenerateRandomPassword(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        private string EncryptPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(password);
                var hash = sha256.ComputeHash(bytes);
                return Convert.ToBase64String(hash);
            }
        }

    }
    }
