using AESMovilAPI.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using System.Dynamic;
using System.Net.Http.Headers;
using System.Text;

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
        protected readonly HttpClient? _client;
        protected readonly string _token;

        protected int _statusCode;
        protected IMemoryCache _memory;

        public BaseController(IConfiguration config, HttpClient? client = null, IMemoryCache cache = null)
        {
            _config = config;
            _token = string.Empty;
            _statusCode = BAD_REQUEST_400;
            _client = client;
            _memory = cache;
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
                    try
                    {
                        response.Success = true;
                        response.Message = response.Message.Equals("Failed") ? "Successfully" : response.Message;
                    }
                    catch (Exception ex)
                    {

                    }
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
                    try
                    {
                        response.Success = true;
                        response.Message = response.Message.Equals("Failed") ? "Successfully" : response.Message;
                    }
                    catch (Exception ex)
                    {

                    }
                    return Ok(response);
            }
        }

        protected async Task<object?> ExecuteGetRequestSAP(string endpoint)
        {
            if (_client != null)
            {
                string baseUrl = _config.GetValue<string>("SAPInterface:Base");
                string mandante = _config.GetValue<string>("SAPInterface:ID");
                string link = baseUrl + "/CIS_" + mandante + "_" + endpoint;

                var queryParams = new Dictionary<string, string>
            {
                { "$expand", "DataSet" },
                { "$format", "json" }
            };

                // Build the URL with query parameters
                var urlWithParams = QueryHelpers.AddQueryString(link, queryParams);
                // Create HttpRequestMessage
                var request = new HttpRequestMessage(HttpMethod.Get, urlWithParams);

                // Define the username and password for Basic Authentication
                var username = _config.GetValue<string>("SAPInterface:Usr");
                var password = _config.GetValue<string>("SAPInterface:Pwd");

                // Create the authentication header value
                var byteArray = Encoding.ASCII.GetBytes($"{username}:{password}");
                var authHeader = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));

                // Set the authorization header
                _client.DefaultRequestHeaders.Authorization = authHeader;

                // Set custom headers
                request.Headers.Add("x-csfr-token", _config.GetValue<string>("SAPInterface:Token"));

                try
                {
                    // Send the GET request
                    var response = await _client.SendAsync(request);

                    // Ensure the request was successful
                    response.EnsureSuccessStatusCode();

                    // Read the response content as a string
                    var responseContent = await response.Content.ReadAsStringAsync();
                    dynamic responseObject = JsonConvert.DeserializeObject<ExpandoObject>(responseContent)!;

                    if (string.IsNullOrEmpty(responseObject.d.Errorcode) || responseObject.d.Errorcode == "0")
                    {
                        return new
                        {
                            data = responseObject.d
                        };
                    }
                }
                catch (HttpRequestException e)
                {

                }
            }

            return null;
        }
        protected async Task<object?> ExecutePostRequest(object postData, string url, bool auth = true, string token = "", bool bearer = true)
        {
            var jsonContent = JsonConvert.SerializeObject(postData);
            var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            if (auth && !string.IsNullOrEmpty(token))
            {
                AuthenticationHeaderValue authHeader;
                if (bearer)
                {
                    authHeader = new AuthenticationHeaderValue("Bearer", token);
                }
                else
                {
                    authHeader = new AuthenticationHeaderValue("Basic", token);
                }

                // Set the authorization header
                _client.DefaultRequestHeaders.Authorization = authHeader;
            }

            try
            {
                // Send the POST request
                var response = await _client.PostAsync(url, httpContent);

                // Ensure the request was successful
                response.EnsureSuccessStatusCode();

                // Read the response content as a string
                var responseContent = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<ExpandoObject>(responseContent)!;
            }
            catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                return UNAUTHORIZED_401;
            }
            catch (HttpRequestException e)
            {

            }

            return null;
        }

        protected void SaveToken(string clave, string token)
        {
            if (_memory != null)
            {
                // Almacena el token en la caché con una expiración de 60 minutos
                var opcionesDeCache = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromHours(24));

                _memory.Set(clave, token, opcionesDeCache);
            }
        }

        protected string? GetToken(string clave)
        {
            string? token = null;
            if (_memory != null)
            {
                _memory.TryGetValue(clave, out token);
            }
            return token;
        }
    }
}
