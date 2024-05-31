using AESMovilAPI.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AESMovilAPI.Controllers
{
    [Authorize]
    [ApiController]
    [RequireHttps]
    public class BaseController : ControllerBase
    {
        protected const int OK_200 = StatusCodes.Status200OK;
        protected const int CREATED_201 = StatusCodes.Status201Created;
        protected const int BAD_REQUEST_400 = StatusCodes.Status400BadRequest;
        protected const int UNAUTHORIZED_401 = StatusCodes.Status401Unauthorized;
        protected const int FORBIDDEN_403 = StatusCodes.Status403Forbidden;
        protected const int NOT_FOUND_404 = StatusCodes.Status404NotFound;
        protected const int SERVICE_UNAVAILABLE_503 = StatusCodes.Status503ServiceUnavailable;

        protected readonly IConfiguration _config;
        protected readonly string _token;

        protected int _statusCode;

        protected BaseController(IConfiguration config)
        {
            _config = config;
            _token = string.Empty;
            _statusCode = BAD_REQUEST_400;
        }

        /// <summary>
        /// Crear respuesta según el <c>_statusCode</c>
        /// </summary>
        /// <typeparam name="T">Objeto data de la respuesta </typeparam>
        /// <param name="response">Informacion de respuesta</param>
        /// <returns></returns>
        protected ObjectResult GetResponse<T>(Response<T> response)
        {
            switch (_statusCode)
            {
                case CREATED_201:      //POST
                    response.Success = true;
                    response.Message = response.Message.Equals("Failed") ? "Successfully" : response.Message;
                    return StatusCode(_statusCode, response);
                case BAD_REQUEST_400:   //POST, GET, Default
                    return BadRequest(response);
                case UNAUTHORIZED_401:
                    return Unauthorized(response);
                case FORBIDDEN_403:    //POST, GET
                    return StatusCode(_statusCode, response);
                case NOT_FOUND_404:     //GET
                    return NotFound(response);
                case SERVICE_UNAVAILABLE_503:
                    return StatusCode(_statusCode, response);
                default:
                    response.Success = true;
                    response.Message = response.Message.Equals("Failed") ? "Successfully" : response.Message;
                    return Ok(response);
            }
        }
    }
}
