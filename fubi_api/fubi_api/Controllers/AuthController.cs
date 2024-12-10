using Dapper;
using fubi_api.Models;
using fubi_api.Utils.Auth;
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
    }
}
