using AESMovilAPI.DTOs;
using AESMovilAPI.Responses;
using ivraes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AESMovilAPI.Controllers
{
    [Route("api/v1/[controller]")]
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
        /// <response code="500">Ha ocurrido un error faltal en el servicio.</response>
        /// <response code="502">Incidente en el servicio.</response>
        // GET: api/v1/technicalclaims/verifier
        [AllowAnonymous]
        [HttpGet]
        [Route("[action]")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [NonAction]
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

        /// <summary>
        /// Creación de reclamos técnicos.
        /// </summary>
        /// <param name="claim">Datos del reaclamo representado por el objeto <see cref="ClaimDto">ClaimDto</see>. Reclamo por falta de suministro.</param>
        /// <returns>Número de reclamo</returns>
        /// <response code="201">Se creó el reclamo.</response>
        /// <response code="400">Error en datos para crear reclamo.</response>
        /// <response code="401">Error por token de autorización.</response>
        /// <response code="500">Ha ocurrido un error faltal en el servicio.</response>
        /// <response code="502">Incidente en el servicio.</response>
        /// <response code="503">Error interno en el proceso de creación del reclamo.</response>
        // POST: api/v1/technicalclaims
        [HttpPost]
        public async Task<IActionResult> Create(ClaimDto claim)
        {
            Response<ClaimResponse> response = new Response<ClaimResponse>();

            if (claim != null && ModelState.IsValid)
            {
                try
                {
                    _statusCode = CREATED_201;
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
                        _statusCode = SERVICE_UNAVAILABLE_503;
                    }
                }
                catch (Exception ex)
                {
                    _statusCode = BAD_REQUEST_400;
                    response.Message = ex.Message;
                }
            }

            return GetResponse(response);
        }
    }
}
