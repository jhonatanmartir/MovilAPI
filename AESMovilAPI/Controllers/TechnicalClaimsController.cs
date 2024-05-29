using AESMovilAPI.DTOs;
using AESMovilAPI.Responses;
using ivraes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AESMovilAPI.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/v1/[controller]")]
    [RequireHttps]
    public class TechnicalClaimsController : BaseController
    {
        private readonly VRAESELSALVADORSoapClient _ivrClient;

        public TechnicalClaimsController(IConfiguration config, VRAESELSALVADORSoapClient ivr) : base(config)
        {
            _ivrClient = ivr;
        }

        /// <summary>
        /// Verifica que el servicio se encuentre funcionando.
        /// </summary>
        /// <returns>Texto</returns>
        /// <response code="200">El servicio esta funcionando.</response>
        /// <response code="202">El servicio esta en pruebas.</response>
        /// <response code="500">Incidente en el servicio.</response>
        // GET: api/v1/technicalclaims/verifier
        [AllowAnonymous]
        [HttpGet]
        [Route("[action]")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult Verifier()
        {
            string result = "Keep calm We good over here!";

            if (true)
            {
                return Accepted(result);
            }
            else
            {
                return Ok(result);
            }
        }

        // POST: api/v1/technicalclaims
        [HttpPost]
        public async Task<IActionResult> Create(ClaimDto claim)
        {
            Response<ClaimResponse> response = new Response<ClaimResponse>();
            int statusCode = StatusCodes.Status200OK;

            if (claim != null && ModelState.IsValid)
            {
                try
                {
                    CrearReclamoResponse ivResponse = await _ivrClient.CrearReclamoAsync(
                    claim.NC.ToString(),
                    claim.OrigenReclamo,
                    claim.TipoReclamo,
                    claim.ComentarioReclamo,
                    claim.ComentarioDireccion,
                    claim.Peligro,
                    claim.VecinosAfectados ? "1" : "0",
                    claim.Empresa,
                    claim.Departamento,
                    claim.Municipio,
                    claim.Usuario,
                    claim.Usuario,
                    claim.Telefono);

                    response.Message = ivResponse.Body.CrearReclamoResult.MENSAJE_ERROR;
                    response.Data = new ClaimResponse
                    {
                        ClaimNumber = ivResponse.Body.CrearReclamoResult.NUMERO_RECLAMO,
                        Reiterations = ivResponse.Body.CrearReclamoResult.REITERACIONES
                    };

                    if (string.IsNullOrEmpty(ivResponse.Body.CrearReclamoResult.NUMERO_RECLAMO))
                    {
                        statusCode = StatusCodes.Status503ServiceUnavailable;
                    }
                }
                catch (Exception ex)
                {
                    statusCode = StatusCodes.Status422UnprocessableEntity;
                }
            }

            return GetResponse(response, statusCode);
        }
    }
}
