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
        protected readonly IConfiguration _config;
        protected readonly string _token;
        protected int _statusCode;

        protected BaseController(IConfiguration config)
        {
            _config = config;
            _token = string.Empty;
            _statusCode = StatusCodes.Status400BadRequest;
        }

        protected ObjectResult GetResponse<T>(Response<T> response)
        {
            switch (_statusCode)
            {
                case StatusCodes.Status201Created:      //POST
                    return StatusCode(StatusCodes.Status201Created, response);
                case StatusCodes.Status400BadRequest:   //POST, GET
                    return BadRequest(response);
                //case StatusCodes.Status401Unauthorized:
                //    return Unauthorized(response);
                case StatusCodes.Status403Forbidden:    //POST, GET
                    return StatusCode(StatusCodes.Status403Forbidden, response);
                case StatusCodes.Status404NotFound:     //GET
                    return NotFound(response);
                //case StatusCodes.Status422UnprocessableEntity:
                //    return UnprocessableEntity(response);
                case StatusCodes.Status503ServiceUnavailable:
                    return StatusCode(StatusCodes.Status503ServiceUnavailable, response);
                default:
                    response.Success = true;
                    response.Message = response.Message.Equals("Failed") ? "Successfully" : response.Message;
                    return Ok(response);
            }
        }

        [Obsolete("NO usar")]
        protected ObjectResult GetResponse<T>(Response<T> response, int statusCode = 200)
        {
            switch (_statusCode)
            {
                case StatusCodes.Status201Created:      //POST
                    return StatusCode(StatusCodes.Status201Created, response);
                case StatusCodes.Status400BadRequest:   //POST, GET
                    return BadRequest(response);
                //case StatusCodes.Status401Unauthorized:
                //    return Unauthorized(response);
                case StatusCodes.Status403Forbidden:    //POST, GET
                    return StatusCode(StatusCodes.Status403Forbidden, response);
                case StatusCodes.Status404NotFound:     //GET
                    return NotFound(response);
                //case StatusCodes.Status422UnprocessableEntity:
                //    return UnprocessableEntity(response);
                case StatusCodes.Status503ServiceUnavailable:
                    return StatusCode(StatusCodes.Status503ServiceUnavailable, response);
                default:
                    response.Success = true;
                    response.Message = response.Message.Equals("Failed") ? "Successfully" : response.Message;
                    return Ok(response);
            }
        }
    }
}
