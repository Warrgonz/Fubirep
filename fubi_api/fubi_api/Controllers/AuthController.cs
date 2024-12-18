using Dapper;
using fubi_api.Models;
using fubi_api.Utils.Auth;
using fubi_api.Utils.Smtp;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.Tokens;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace fubi_api.Controllers
{
    [Route("/api/[controller]")]
    [ApiController]
    public class AuthController : Controller
    {

        private readonly IConfiguration _conf;
        private readonly IAuth _auth;

        public AuthController(IConfiguration conf, IAuth auth)
        {
            _conf = conf;
            _auth = auth;
        }

        [HttpPost]
        [Route("IniciarSesion")]
        public IActionResult IniciarSesion(User model)
        {
            try
            {

                using (var context = new SqlConnection(_conf.GetSection("ConnectionStrings:DefaultConnection").Value))
                {
                    var parametros = new { correo = model.correo, contrasena = model.contrasena };
                    var respuesta = new Respuesta();

                    var usuario = context.QueryFirstOrDefault<User>("ObtenerAuth", parametros, commandType: CommandType.StoredProcedure);

                    if (usuario == null)
                    {
                        // El usuario no existe
                        respuesta.Codigo = -1;
                        respuesta.Mensaje = "El correo o contraseña son incorrectos, por favor inténtelo de nuevo.";
                        return Ok(respuesta);
                    }

                    if (usuario.activo == 0)
                    {
                        // El usuario está inactivo
                        respuesta.Codigo = -1;
                        respuesta.Mensaje = "El usuario está inactivo. Por favor, contacte al administrador local.";
                        return Ok(respuesta);
                    }

                    // El usuario es válido y activo
                    usuario.Token = GenerarToken(usuario);
                    respuesta.Codigo = 0;
                    respuesta.Contenido = usuario;
                    return Ok(respuesta);
                }

            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Codigo = -1,
                    Mensaje = "Error interno del servidor",
                    Detalles = ex.Message 
                });
            } 
        }

        [HttpPost]
        [Route("RecuperarAcceso")]
        public async Task<IActionResult> RecuperarAcceso([FromBody] User model, [FromServices] IMessage emailService)
        {
            try
            {
                if (string.IsNullOrEmpty(model.correo))
                {
                    return BadRequest(new { Codigo = -1, Mensaje = "El correo es obligatorio." });
                }

                using (var context = new SqlConnection(_conf.GetSection("ConnectionStrings:DefaultConnection").Value))
                {
                    // Verificar si el correo existe
                    var usuario = await context.QueryFirstOrDefaultAsync<User>(
                        "VerificarCorreo",
                        new { Correo = model.correo },
                        commandType: CommandType.StoredProcedure);

                    if (usuario == null)
                    {
                        return NotFound(new { Codigo = -1, Mensaje = "El correo no está registrado." });
                    }

                    // Generar token
                    var token = Guid.NewGuid().ToString();
                    var link = $"{_conf.GetSection("Variables:client").Value}Home/RestablecerContrasena?token={token}";

                    // Guardar el token en la tabla de usuarios
                    await context.ExecuteAsync(
                        "usp_GuardarTokenEnUsuario",
                        new { Token = token, Correo = model.correo },
                        commandType: CommandType.StoredProcedure);

                    // Enviar correo
                    emailService.SendEmail(
                        "Recuperación de contraseña - Fubiredip",
                        $"Hola {usuario.nombre},<br><br>Hemos recibido tu solicitud para restaurar tu contraseña.<br>Haz clic en el siguiente enlace para continuar:<br><a href='{link}'>Restaurar Contraseña</a><br><br>Gracias,<br>Fubiredip.",
                        usuario.correo
                    );

                    return Ok(new { Codigo = 0, Mensaje = "Correo enviado correctamente." });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Codigo = -1, Mensaje = "Error interno del servidor", Detalles = ex.Message });
            }
        }

        [HttpPost]
        [Route("RestablecerContrasena")]
        public async Task<IActionResult> RestablecerContrasena([FromBody] User model)
        {
            try
            {
                using (var context = new SqlConnection(_conf.GetSection("ConnectionStrings:DefaultConnection").Value))
                {
                    // Validar token
                    var usuario = await context.QueryFirstOrDefaultAsync<User>(
                        "usp_ValidarTokenUsuario",
                        new { Token = model.Token },
                        commandType: CommandType.StoredProcedure);

                    if (usuario == null)
                    {
                        return BadRequest(new { Codigo = -1, Mensaje = "Token inválido o expirado." });
                    }

                    if (model.contrasena != model.contrasenaConfirmar)
                    {
                        return BadRequest(new { Codigo = -1, Mensaje = "Las contraseñas no coinciden." });
                    }

                    // Actualizar contraseña y limpiar token
                    var hashedPassword = _auth.Hashear(model.contrasena);
                    await context.ExecuteAsync(
                        "usp_ActualizarContrasenaYLimpiarToken",
                        new { Contrasena = hashedPassword, Token = model.Token },
                        commandType: CommandType.StoredProcedure);

                    return Ok(new { Codigo = 0, Mensaje = "Contraseña actualizada correctamente." });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Codigo = -1, Mensaje = "Error interno del servidor", Detalles = ex.Message });
            }
        }

        [HttpGet("ObtenerUsuarioPorToken")]
        public IActionResult ObtenerUsuarioPorToken(string token)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(token))
                {
                    return StatusCode(410, new Respuesta { Codigo = -1, Mensaje = "El token ha caducado o es inválido. Solicita un nuevo enlace." });
                }

                using (var context = new SqlConnection(_conf.GetSection("ConnectionStrings:DefaultConnection").Value))
                {
                    var query = "ObtenerUsuarioPorToken";
                    var usuario = context.QueryFirstOrDefault<User>(
                        query,
                        new { Token = token },
                        commandType: CommandType.StoredProcedure);

                    if (usuario == null)
                    {
                        return StatusCode(404, new Respuesta { Codigo = -1, Mensaje = "El link ingresado es invalido" });
                    }

                    var tiempoTranscurrido = DateTime.Now - usuario.token_generado.Value;

                    if (tiempoTranscurrido.TotalMinutes > 20)
                    {
                        return TokenExpirado(usuario);
                    }

                    return Ok(new Respuesta { Codigo = 0, Contenido = usuario });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new Respuesta { Codigo = -1, Mensaje = $"Error interno: {ex.Message}" });
            }
        }

        [HttpPost("ActualizarContrasena")]
        public IActionResult ActualizarContrasena([FromBody] User model)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(model.Token))
                {
                    return StatusCode(410, new Respuesta { Codigo = -1, Mensaje = "El token ha caducado. Solicita un nuevo enlace." });
                }

                using (var connection = new SqlConnection(_conf.GetConnectionString("DefaultConnection")))
                {
                    var parameters = new DynamicParameters();
                    parameters.Add("@Token", model.Token);
                    parameters.Add("@Contrasena", model.contrasena);

                    var rowsAffected = connection.Execute("ActualizarContrasenaPorToken", parameters, commandType: CommandType.StoredProcedure);

                    if (rowsAffected > 0)
                    {
                        return Ok(new Respuesta { Codigo = 0, Mensaje = "Contraseña actualizada correctamente." });
                    }
                    else
                    {
                        return NotFound(new Respuesta { Codigo = -1, Mensaje = "El token es inválido o el usuario no existe." });
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new Respuesta { Codigo = -1, Mensaje = $"Error interno: {ex.Message}" });
            }
        }

        // Utilidades
        private string GenerarToken(User model)
        {
            string SecretKey = _conf.GetSection("Variables:Key").Value!;

            List<Claim> claims = new List<Claim>();
            claims.Add(new Claim("cedula", model.cedula.ToString()));
            claims.Add(new Claim("id_rol", model.rol.ToString()));

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SecretKey));
            var cred = new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature);

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(20),
                signingCredentials: cred);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private IActionResult TokenExpirado(User usuario)
        {
            try
            {
                using (var context = new SqlConnection(_conf.GetConnectionString("DefaultConnection")))
                {
                    var parameters = new { UserId = usuario.id_usuario };

                    // Llamamos al stored procedure para "expirar" el token
                    context.Execute("TokenExpirado", parameters, commandType: CommandType.StoredProcedure);
                }

                // Si la expiración es exitosa, devolvemos un código 410 (Gone) con el mensaje adecuado
                return StatusCode(410, new Respuesta { Codigo = -1, Mensaje = "El token ha expirado. Solicita un nuevo enlace." });
            }
            catch (Exception ex)
            {
                // En caso de error, devolvemos un mensaje de error
                return StatusCode(500, new Respuesta { Codigo = -1, Mensaje = $"Error al procesar la expiración del token: {ex.Message}" });
            }
        }


    }
}
