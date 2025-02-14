using AESMovilAPI.DTOs;
using AESMovilAPI.Examples;
using AESMovilAPI.Responses;
using AESMovilAPI.Services;
using AESMovilAPI.Utilities;
using ivraes;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Swashbuckle.AspNetCore.Filters;
using System.Net.Mime;
using System.Text;

namespace AESMovilAPI.Controllers
{
    [Route("api/v1/[controller]")]
    public class TechnicalClaimController : BaseController<TechnicalClaimController>
    {
        private readonly VRAESELSALVADORSoapClient _ivrClient;
        private readonly ivradms.VRAESELSALVADORSoapClient _ivradmsClient;

        public TechnicalClaimController(IConfiguration config, LoggerService<TechnicalClaimController> logger, IHttpClientFactory httpClientFactory, VRAESELSALVADORSoapClient ivr, ivradms.VRAESELSALVADORSoapClient ivradms) : base(config, logger, httpClientFactory)
        {
            _ivrClient = ivr;
            _ivradmsClient = ivradms;
        }

        /// <summary>
        /// Creación de reclamos técnicos.
        /// </summary>
        /// <param name="claim">Datos del reaclamo representado por el objeto <see cref="Claim">Claim</see>. Reclamo por falta de suministro.</param>
        /// <returns>Número de reclamo</returns>
        /// <response code="201">Se creó el reclamo con éxito.</response>
        /// <response code="400">Datos incorrectos para crear reclamo.</response>
        /// <response code="401">Error por token de autorización.</response>
        /// <response code="500">Error inesperado en el servicio. Intente nuevamente en unos minutos.</response>
        /// <response code="502">Servicio dependiente no respondió correctamente.</response>
        /// <response code="503">Servicio no disponible en este momento.</response>
        // POST: api/v1/technicalclaims
        [HttpPost]
        public async Task<IActionResult> Create(Claim claim)
        {
            Response<ClaimResponse> response = new Response<ClaimResponse>();
            var isIVRADMS = _config.GetValue<bool>(Constants.CONF_OMS_IVRADMS);

            if (claim != null && ModelState.IsValid)
            {
                try
                {
                    _statusCode = CREATED_201;
                    if (!isIVRADMS)
                    {
                        ivraes.CrearReclamoResponse ivResponse = await _ivrClient.CrearReclamoAsync(
                        claim.Contrato.ToString(),
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
                            _statusCode = BAD_GATEWAY_502;
                        }
                    }
                    else
                    {
                        ivradms.CrearReclamoResponse ivResponse = await _ivradmsClient.CrearReclamoAsync(
                        claim.Contrato.ToString(),
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
                            _statusCode = BAD_GATEWAY_502;
                        }
                    }

                }
                catch (Exception ex)
                {
                    _statusCode = INTERNAL_ERROR_500;
                    response.Message = ex.Message;
                    _log.Err(ex);
                }
            }

            return GetResponse(response);
        }

        /// <summary>
        /// Confirmacion de restablecimiento de suministro.
        /// </summary>
        /// <param name="customer">Confirmacion del cliente representado por el objeto <see cref="Callback">Callback</see>.</param>
        /// <returns>Correcto o Incorrecto.</returns>
        /// <response code="200">Solicitud completada con éxito.</response>
        /// <response code="400">Consulta con datos incorrectos.</response>
        /// <response code="401">Error por token de autorización.</response>
        /// <response code="500">Error inesperado en el servicio. Intente nuevamente en unos minutos.</response>
        /// <response code="502">Servicio dependiente no respondió correctamente.</response>
        /// <response code="503">Servicio no disponible en este momento.</response>
        // POST: api/v1/ivrcallback
        [SwaggerRequestExample(typeof(Callback), typeof(CallbackExample))]
        [HttpPost]
        [Route("[action]")]
        public async Task<IActionResult> IVRCallback(Callback customer)
        {
            Response<string> response = new Response<string>();

            if (customer != null && ModelState.IsValid)
            {
                try
                {
                    var jsonContent = JsonConvert.SerializeObject(customer.Option);
                    using var httpContent = new StringContent(jsonContent, Encoding.UTF8, MediaTypeNames.Application.Json);

                    try
                    {
                        var url = $"{_config.GetValue<string>(Constants.CONF_OMS_BASE)}/oms/external/api/ServiceType/Electric/Callback/{customer.Code}/Status";
                        var byteArray = Encoding.ASCII.GetBytes($"{_config.GetValue<string>(Constants.CONF_OMS_USER)}:{_config.GetValue<string>(Constants.CONF_OMS_PASSWORD)}");
                        var token = Convert.ToBase64String(byteArray);

                        // Send the POST request
                        dynamic? result = await ExecutePostRequestInsecure(customer.Option, url, true, token, false);

                        if (result == Constants.SUCCESS)
                        {
                            _statusCode = OK_200;
                        }
                        else
                        {
                            _statusCode = NOT_FOUND_404;
                        }
                    }
                    catch (HttpRequestException e)
                    {
                        _statusCode = BAD_GATEWAY_502;
                        _log.Err(e);
                    }
                }
                catch (Exception ex)
                {
                    _statusCode = INTERNAL_ERROR_500;
                    response.Message = ex.Message;
                    _log.Err(ex);
                }
            }

            return GetResponse(response);
        }
    }
}
