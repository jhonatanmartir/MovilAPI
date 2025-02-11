using AESMovilAPI.DTOs;
using AESMovilAPI.Responses;
using AESMovilAPI.Services;
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
        /// <param name="id">Numero de Cuenta contrato</param>
        /// <returns>Saldo actual del cliente.</returns>
        /// <response code="200">Correcto.</response>
        /// <response code="400">Consulta no corresponde.</response>
        /// <response code="401">Error por token de autorización.</response>
        /// <response code="404">No se encontró deuda.</response>
        /// <response code="500">Ha ocurrido un error critico en el servicio.</response>
        /// <response code="502">Incidente en el servicio.</response>
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(string id)
        {
            var response = new Response<BalanceResponse>();

            if (!string.IsNullOrEmpty(id.Trim()))
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
