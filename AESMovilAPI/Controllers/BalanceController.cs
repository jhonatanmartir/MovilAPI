using AESMovilAPI.DTOs;
using AESMovilAPI.Responses;
using AESMovilAPI.Services;
using AESMovilAPI.Utilities;
using Microsoft.AspNetCore.Mvc;

namespace AESMovilAPI.Controllers
{
    [Route("api/v1/[controller]")]
    public class BalanceController : BaseController<BalanceController>
    {
        public BalanceController(IConfiguration config, LoggerService<BalanceController> logger, IHttpClientFactory httpClientFactory) : base(config, logger, httpClientFactory)
        {

        }

        /// <summary>
        /// Consulta de saldo.
        /// </summary>
        /// <param name="id">Número de Cuenta Contrato</param>
        /// <returns>Detalle del saldo actual del cliente.</returns>
        /// <response code="200">Solicitud completada con éxito.</response>
        /// <response code="400">Consulta con datos incorrectos.</response>
        /// <response code="401">Error por token de autorización.</response>
        /// <response code="404">No se encontraron datos.</response>
        /// <response code="500">Error inesperado en el servicio. Intente nuevamente en unos minutos.</response>
        /// <response code="502">Servicio dependiente no respondió correctamente.</response>
        /// <response code="503">Servicio no disponible en este momento.</response>
        [HttpGet("{id}/{channel?}")]
        public async Task<IActionResult> Get(string id, string? channel)
        {
            var response = new Response<BalanceResponse>();

            if (Helper.IsCuentaContrato(id))
            {
                BillDto? bill = await GetInvoiceData(id);

                if (bill != null)
                {
                    var data = new BalanceResponse();
                    data.Amount = bill.Amount - bill.MayoralAmount - bill.ReconnectionAmount;
                    data.TotalAmount = bill.Amount;
                    data.ReconnectionAmount = bill.ReconnectionAmount;
                    data.MayoralAmount = bill.MayoralAmount;
                    data.ExpirationDate = bill.ExpirationDate.ToString("dd/MM/yyyy");
                    data.DocumentNumber = bill.DocumentNumber.ToString();

                    _statusCode = OK_200;
                    response.Data = data;
                }
                else
                {
                    _statusCode = NOT_FOUND_404;
                }
            }
            else
            {
                _statusCode = BAD_REQUEST_400;
            }

            return GetResponse(response);
        }
    }
}
