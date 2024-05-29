using AESMovilAPI.DTOs;
using AESMovilAPI.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AESMovilAPI.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/v1/[controller]")]
    [RequireHttps]
    public class PaymentsController : BaseController
    {
        private readonly HttpClient _httpClient;
        public PaymentsController(IConfiguration config, HttpClient httpClient) : base(config)
        {
            _httpClient = httpClient;
        }

        [HttpPost]
        public IActionResult Create(PaymentDto data)
        {
            int statusCode = StatusCodes.Status422UnprocessableEntity;
            Response<string> response = new Response<string>();

            if (data != null && ModelState.IsValid)
            {
                switch (data.Collector.ToUpper())
                {
                    case "PAGADITO":
                        statusCode = StatusCodes.Status200OK;
                        response.Data = GetPaywayLink(data.NC.ToString());
                        break;
                    case "PAYWAY":
                        statusCode = StatusCodes.Status200OK;
                        break;
                    default: break;
                }
            }

            return GetResponse(response, statusCode);
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
