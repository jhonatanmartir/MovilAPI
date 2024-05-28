using Microsoft.AspNetCore.Mvc;

namespace AESMovilAPI.Controllers
{
    [ApiController]
    [RequireHttps]
    public class BaseController : ControllerBase
    {
        protected readonly IConfiguration _config;
        protected readonly string _token;

        public BaseController(IConfiguration config)
        {
            _config = config;
            _token = "";
        }
    }
}
