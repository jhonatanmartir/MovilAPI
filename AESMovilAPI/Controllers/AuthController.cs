using AESMovilAPI.DTOs;
using AESMovilAPI.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace AESMovilAPI.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    [RequireHttps]
    public class AuthController : BaseController
    {
        public AuthController(IConfiguration config) : base(config)
        {
        }

        // POST: api/v1/auth/login
        [AllowAnonymous]
        [HttpPost]
        [Route("[action]")]
        public IActionResult Login(LoginDto auth)
        {
            int statusCode = StatusCodes.Status401Unauthorized;
            Response<string> response = new Response<string>();

            if (auth != null)
            {
                var credentialBytes = Convert.FromBase64String(auth.Key);
                var credentials = Encoding.UTF8.GetString(credentialBytes).Split(new[] { ':' }, 2);
                var username = credentials[0];
                var password = credentials[1];

                //password = Util.GetSHA1(password);
                if (IsAuthorized(username, password))
                {
                    string token = "";
                    try
                    {
                        token = GenerateJwtToken(username);
                        statusCode = StatusCodes.Status201Created;
                    }
                    catch (Exception ex)
                    {
                        statusCode = StatusCodes.Status422UnprocessableEntity;
                    }

                    response.Data = "Bearer " + token;
                }
            }

            return GetResponse<string>(response, statusCode);
        }

        private bool IsAuthorized(string user, string pwd)
        {
            var users = _config.GetSection("Authorized").Get<List<KeyValuePair<string, string>>>();
            var result = users?.Contains(new KeyValuePair<string, string>(user, pwd));

            return result != null ? true : false;
        }

        private string GenerateJwtToken(string user)
        {
            //var mkey = GenerateKey("jhonatan martir");
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes("6e7a10f083b54c551425112f0d0180da5c9bc2fe18daedd8dd1e338444ec29db");
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Issuer = "www.movilaesweb.com",
                Audience = "www.movilaesweb.com",
                Subject = new ClaimsIdentity(new Claim[]
                {
                new Claim(ClaimTypes.Name, user),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.Email, "creativa.jmartir.c@aes.com")
                }),
                Expires = DateTime.UtcNow.AddYears(10),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        private byte[] GenerateKey(string value, int keySize = 256)
        {
            byte[] key;
            using (var rng = new RNGCryptoServiceProvider())
            {
                key = new byte[keySize / 8];
                rng.GetBytes(key);
            }

            byte[] data = System.Text.Encoding.UTF8.GetBytes(value);
            byte[] hash;

            using (var hmac = new HMACSHA256(key))
            {
                hash = hmac.ComputeHash(data);
                Console.WriteLine(BitConverter.ToString(hash).Replace("-", "").ToLower());
            }

            return hash;
        }
    }
}
