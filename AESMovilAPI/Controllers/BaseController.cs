using AESMovilAPI.Responses;
using Microsoft.AspNetCore.Mvc;

namespace AESMovilAPI.Controllers
{
    [ApiController]
    [RequireHttps]
    public class BaseController : ControllerBase
    {
        protected readonly IConfiguration _config;
        protected readonly string _token;

        protected BaseController(IConfiguration config)
        {
            _config = config;
            _token = "";
        }

        protected ObjectResult GetResponse<T>(Response<T> response, int code = 200)
        {
            switch (code)
            {
                case StatusCodes.Status503ServiceUnavailable:
                    return StatusCode(StatusCodes.Status503ServiceUnavailable, response);
                case StatusCodes.Status422UnprocessableEntity:
                    return UnprocessableEntity(response);
                case StatusCodes.Status401Unauthorized:
                    return Unauthorized(response);
                default:
                    response.Success = true;
                    response.Message = response.Message.Equals("Failed") ? "Successfully" : response.Message;
                    return Ok(response);
            }
        }
    }
}
