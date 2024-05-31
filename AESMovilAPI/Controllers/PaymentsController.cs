using AESMovilAPI.DTOs;
using AESMovilAPI.Responses;
using Microsoft.AspNetCore.Mvc;

namespace AESMovilAPI.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    [Route("api/v1/[controller]")]
    public class PaymentsController : BaseController
    {
        private readonly HttpClient _httpClient;
        public PaymentsController(IConfiguration config, HttpClient httpClient) : base(config)
        {
            _httpClient = httpClient;
        }

        /// <summary>
        /// Generar un link de pago.
        /// </summary>
        /// <param name="data">Objeto que representa <see cref="PaymentDto">PaymentDto</see> para crear links de pago.</param>
        /// <returns>Link de pago.</returns>
        /// <response code="201">Link creado.</response>
        /// <response code="400">Datos incompletos.</response>
        /// <response code="401">Error por token de autorización.</response>
        /// <response code="404">No se genero el link.</response>
        /// <response code="500">Incidente en el servicio.</response>
        [HttpPost]
        public IActionResult Create(PaymentDto data)
        {
            Response<string> response = new Response<string>();

            if (data != null && ModelState.IsValid)
            {
                _statusCode = NOT_FOUND_404;
                switch (data.Collector.ToUpper())
                {
                    case "PAGADITO":
                        response.Data = GetPaywayLink(data.NC.ToString());
                        break;
                    case "PAYWAY":
                        response.Data = GetPagaditoLink(data.NC.ToString());
                        break;
                    default: break;
                }
                _statusCode = string.IsNullOrEmpty(response.Data) ? NOT_FOUND_404 : CREATED_201;
            }

            return GetResponse(response);
        }

        private string GetPaywayLink(string nc)
        {

            return nc;
        }

        private string GetPagaditoLink(string nc)
        {

            return nc;
        }
    }
}
