using AESMovilAPI.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
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

        public BaseController(IConfiguration config, HttpClient? client = null)
        {
            _config = config;
            _token = string.Empty;
            _statusCode = BAD_REQUEST_400;
            _client = client;
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
                string baseUrl = "https://aes-cf-gcp-1kg8o7mu.it-cpi017-rt.cfapps.us30.hana.ondemand.com/gw/odata/SAP/";
                string mandante = "CCG160";
                string link = baseUrl + "CIS_" + mandante + "_" + endpoint;

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
                var username = "sb-5c453da1-0024-4006-b300-e197893b4667!b2748|it-rt-aes-cf-gcp-1kg8o7mu!b2560";
                var password = "596b8c78-a140-4dea-9961-50efa79000a5$RtXYp7cf2Jl36e5SWod9iHCwUAtPpERsIi8qFGA6YUE=";

                // Create the authentication header value
                var byteArray = Encoding.ASCII.GetBytes($"{username}:{password}");
                var authHeader = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));

                // Set the authorization header
                _client.DefaultRequestHeaders.Authorization = authHeader;

                // Set custom headers
                request.Headers.Add("x-csfr-token", "c9HO1hYCsB6KRhAIDPUT0lKXxyLyYWXH");

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
    }
}
