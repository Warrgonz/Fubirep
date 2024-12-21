using Dapper;
using fubi_api.Models;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace fubi_api.Controllers
{
    [Route("/api/[controller]")]
    [ApiController]
    public class ErrorController : Controller
    {

        private readonly IConfiguration _conf;

        public ErrorController(IConfiguration conf)
        {
            _conf = conf;
        }

        [HttpPost]
        [Route("RegistrarError")]
        public IActionResult RegistrarError()
        {
            var exc = HttpContext.Features.Get<IExceptionHandlerFeature>();

            using (var context = new SqlConnection(_conf.GetSection("ConnectionStrings:DefaultConnection").Value))
            {
                var cedula = ObtenerUsuarioToken();
                var Mensaje = exc!.Error.Message;
                var Origen = exc.Path;

                context.Execute("RegistrarError", new { cedula, Mensaje, Origen });

                var respuesta = new Respuesta();
                respuesta.Codigo = -500;
                respuesta.Mensaje = "Se presentó un problema en el sistema";
                return Ok(respuesta);
            }
        }

        private long ObtenerUsuarioToken()
        {
            if (User.Claims.Count() > 0)
            {
                var Consecutivo = User.Claims.Select(Claim => new { Claim.Type, Claim.Value })
                    .FirstOrDefault(x => x.Type == "cedula")!.Value;

                return long.Parse(Consecutivo);
            }

            return 0;
        }
    }
}
