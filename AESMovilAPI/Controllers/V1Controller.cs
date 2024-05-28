using AESMovilAPI.DTOs;
using AESMovilAPI.Responses;
using Microsoft.AspNetCore.Mvc;

namespace AESMovilAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [RequireHttps]
    public class V1Controller : BaseController
    {
        public V1Controller(IConfiguration config) : base(config)
        {
        }

        // GET: MovilController/Verifier
        [HttpGet]
        [Route("[action]")]
        public IActionResult Verifier()
        {
            string result = "Keep calm We good over here!";

            if (true)
            {
                return Ok(result);
            }
            else
            {
                return Accepted(result);
            }
        }

        // POST: MovilController/SetReclamo
        [HttpPost]
        [Route("[action]")]
        public IActionResult SetReclamo(ClaimDto claim)
        {
            Response<ClaimResponse> response = new Response<ClaimResponse>();

            if (claim != null && ModelState.IsValid)
            {
                response.Success = true;
            }

            if (response.Success)
            {
                return Ok(response);
            }
            else
            {
                return BadRequest(response);
            }
        }
    }
}
