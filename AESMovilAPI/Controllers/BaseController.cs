using AESMovilAPI.DTOs;
using AESMovilAPI.Filters;
using AESMovilAPI.Responses;
using AESMovilAPI.Services;
using AESMovilAPI.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.CSharp.RuntimeBinder;
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
    [ServiceFilter(typeof(ActionExecutionFilter))]
    public class BaseController<T> : ControllerBase
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

        protected readonly LoggerService<T> _log;

        public BaseController(IConfiguration config, LoggerService<T> logger, IHttpClientFactory? httpClientFactory = null, IMemoryCache cache = null)
        {
            _config = config;
            _token = string.Empty;
            _statusCode = BAD_REQUEST_400;
            _client = httpClientFactory != null ? httpClientFactory.CreateClient(Constants.HTTP_CLIENT_NAME) : null;
            _memory = cache;
            _log = logger;
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
                        _log.Err(ex);
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
                        _log.Err(ex);
                    }
                    return Ok(response);
            }
        }

        #region "Client"
        protected async Task<object?> ExecuteGetRequestSAP(string endpoint, Dictionary<string, string>? queryParams = null, bool overrideUrl = false)
        {
            if (_client != null)
            {
                string baseUrl = _config.GetValue<string>(Constants.CONF_SAP_BASE);
                string mandante = _config.GetValue<string>(Constants.CONF_SAP_ENVIRONMENT);
                string link = baseUrl + "/gw/odata/SAP/CIS" + mandante + "_" + endpoint;

                if (overrideUrl)
                {
                    link = endpoint;
                }

                if (queryParams == null)
                {
                    queryParams = new Dictionary<string, string>
                    {
                        { "$expand", "DataSet" },
                        { "$format", "json" }
                    };
                }


                // Build the URL with query parameters
                var urlWithParams = QueryHelpers.AddQueryString(link, queryParams);
                // Create HttpRequestMessage
                var request = new HttpRequestMessage(HttpMethod.Get, urlWithParams);

                // Define the username and password for Basic Authentication
                var username = _config.GetValue<string>(Constants.CONF_SAP_USER);
                var password = _config.GetValue<string>(Constants.CONF_SAP_PASSWORD);

                // Create the authentication header value
                var byteArray = Encoding.ASCII.GetBytes($"{username}:{password}");
                var authHeader = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));

                // Set the authorization header
                _client.DefaultRequestHeaders.Authorization = authHeader;

                // Set custom headers
                //request.Headers.Add("x-csfr-token", _config.GetValue<string>(Constants.CONF_SAP_TOKEN));

                try
                {
                    // Send the GET request
                    var response = await _client.SendAsync(request);

                    // Ensure the request was successful
                    response.EnsureSuccessStatusCode();

                    // Read the response content as a string
                    var responseContent = await response.Content.ReadAsStringAsync();
                    try
                    {
                        dynamic responseObject = JsonConvert.DeserializeObject<ExpandoObject>(responseContent)!;

                        try
                        {
                            if (string.IsNullOrEmpty(responseObject.d.Errorcode) || responseObject.d.Errorcode == "0")
                            {
                                return new
                                {
                                    data = responseObject.d
                                };
                            }
                        }
                        catch (RuntimeBinderException ex)
                        {
                            if (responseObject.d.MessageType == "0")
                            {
                                return new
                                {
                                    data = responseObject.d
                                };
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        try
                        {
                            // Leer el contenido como un array de bytes
                            var pdfBytes = await response.Content.ReadAsByteArrayAsync();
                            return pdfBytes;
                        }
                        catch (Exception e)
                        {
                            _log.Err(e);
                        }
                        _log.Err(ex);
                    }
                }
                catch (HttpRequestException e)
                {
                    _log.Err(e);
                }
            }

            return null;
        }
        protected async Task<object?> ExecuteGetRequestRPToken(string endpoint)
        {
            string result = string.Empty;

            if (_client != null)
            {
                string baseUrl = _config.GetValue<string>(Constants.CONF_SAP_BASE);
                string mandante = _config.GetValue<string>(Constants.CONF_SAP_ENVIRONMENT);
                string link = baseUrl + "/http/" + mandante + endpoint;

                // Create HttpRequestMessage
                var request = new HttpRequestMessage(HttpMethod.Get, new Uri(link));
                var authHeader = new AuthenticationHeaderValue("Basic", _config.GetValue<string>(Constants.CONF_SAP_REAL_PAYMENT_TOKEN));

                // Set the authorization header
                _client.DefaultRequestHeaders.Authorization = authHeader;

                // Set custom headers
                request.Headers.Add("x-csrf-token", "fetch");
                try
                {
                    // Send the GET request
                    var response = await _client.SendAsync(request);

                    // Ensure the request was successful
                    response.EnsureSuccessStatusCode();

                    if (response.Headers.TryGetValues("x-csrf-token", out var token))
                    {
                        result = token.ElementAt(0);
                        SaveToken(Constants.RP_TOKEN, result);
                    }
                }
                catch (HttpRequestException e)
                {
                    _log.Err(e);
                }
            }

            return result;
        }
        protected async Task<object?> ExecutePostRequestRP(object postData, string endpoint)
        {
            if (_client != null)
            {
                string baseUrl = _config.GetValue<string>(Constants.CONF_SAP_BASE);
                string mandante = _config.GetValue<string>(Constants.CONF_SAP_ENVIRONMENT);
                string link = baseUrl + "/http/" + mandante + endpoint;
                var jsonContent = JsonConvert.SerializeObject(postData);
                var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                // Create HttpRequestMessage
                var request = new HttpRequestMessage(HttpMethod.Post, new Uri(link))
                {
                    Content = httpContent
                };
                var authHeader = new AuthenticationHeaderValue("Basic", _config.GetValue<string>(Constants.CONF_SAP_REAL_PAYMENT_TOKEN));

                // Set the authorization header
                _client.DefaultRequestHeaders.Authorization = authHeader;

                string? token = GetToken(Constants.RP_TOKEN);
                if (string.IsNullOrEmpty(token))
                {
                    token = (string?)await ExecuteGetRequestRPToken(endpoint);
                }

                // Set custom headers
                request.Headers.Add("x-csrf-token", token);

                try
                {
                    // Send the GET request
                    var response = await _client.SendAsync(request);
                    try
                    {
                        // Ensure the request was successful
                        response.EnsureSuccessStatusCode();
                    }
                    catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Forbidden)
                    {
                        token = GetToken(Constants.RP_TOKEN);
                        if (string.IsNullOrEmpty(token))
                        {
                            token = (string?)await ExecuteGetRequestRPToken(endpoint);
                        }

                        // Set custom headers
                        request.Headers.Add("x-csrf-token", token);
                        response = await _client.SendAsync(request);

                        // Ensure the request was successful
                        response.EnsureSuccessStatusCode();
                        _log.Err(ex);
                    }

                    // Read the response content as a string
                    var responseContent = await response.Content.ReadAsStringAsync();
                    responseContent = Helper.CleanXml(responseContent, "http://sap.com/xi/SAPGlobal/Global");
                    var responseObject = Helper.DeserializeXml<CashPointOpenItemSummaryByElementsResponseMessage>(responseContent);

                    return responseObject;
                }
                catch (HttpRequestException e)
                {
                    _log.Err(e);
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
                _log.Err(e);
            }

            return null;
        }
        protected async Task<object?> ExecutePostRequestInsecure(object postData, string url, bool auth = true, string token = "", bool bearer = true)
        {
            var jsonContent = JsonConvert.SerializeObject(postData);
            var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");
            // Usar validación personalizada temporal
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
            };
            using var client = new HttpClient(handler);

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
                client.DefaultRequestHeaders.Authorization = authHeader;
            }

            try
            {
                // Send the POST request
                var response = await client.PostAsync(url, httpContent);

                // Ensure the request was successful
                response.EnsureSuccessStatusCode();

                // Read the response content as a string
                return await response.Content.ReadAsStringAsync();
            }
            catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                return UNAUTHORIZED_401;
            }
            catch (HttpRequestException e)
            {
                _log.Err(e);
            }

            return null;
        }

        #endregion
        #region "Commons"
        protected async Task<BillDto?> GetInvoiceData(string nc)
        {
            var data = new
            {
                Header = "",
                Body = new
                {
                    CashPointOpenItemSummaryByElementsQueryMessage = new
                    {
                        MessageHeader = new
                        {
                            CreationDateTime = "",
                            ID = new
                            {
                                text = ""
                            },
                            ReferenceID = new
                            {
                                text = ""
                            },
                            SenderParty = new
                            {
                                InternalID = new
                                {
                                    text = ""
                                }
                            },
                            RecipientParty = new
                            {
                                InternalID = new
                                {
                                    text = ""
                                }
                            }
                        },
                        CashPointOpenItemSummaryByElementsQuery = new
                        {
                            CashPointReferenceID = new
                            {
                                text = _config.GetValue<string>(Constants.CONF_REAL_PAYMENT_CASH_POINT)
                            },
                            CashPointOfficeReferenceID = new
                            {
                                text = _config.GetValue<string>(Constants.CONF_REAL_PAYMENT_CASH_POINT_OFFICE)
                            },
                            ReportingCurrency = "USD",
                            SelectionByContractAccountID = nc
                        }
                    }
                }
            };

            BillDto? result = null;

            try
            {
                CashPointOpenItemSummaryByElementsResponseMessage values = (CashPointOpenItemSummaryByElementsResponseMessage)await ExecutePostRequestRP(data, "OpenItemSummaryByElements");

                if (values != null)
                {
                    bool mayoral = false;
                    bool reconnection = false;
                    decimal amount = 0;
                    DateTime dueDate = DateTime.Now;
                    string company = "";
                    string name = "";
                    long documentNumber = 0;

                    if (values.CashPointOpenItemSummary?.CashPointOpenItems != null)
                    {
                        foreach (var item in values.CashPointOpenItemSummary.CashPointOpenItems)
                        {
                            try
                            {
                                amount += item.OpenAmount.Value;

                                if (item.OpenItemTransactionDescription.Value == "ALCA")
                                {
                                    mayoral = true;
                                }
                                if (item.OpenItemTransactionDescription.Value == "RECO")
                                {
                                    reconnection = true;
                                }

                                dueDate = item.DueDate;
                                company = item.PaymentFormID.Split("|")[0];
                                name = item.PaymentFormID.Split("|")[1];
                                documentNumber = string.IsNullOrEmpty(item.InvoiceID) ? 0 : long.Parse(item.InvoiceID);
                            }
                            catch (Exception ex)
                            {

                            }
                        }
                    }

                    result = new BillDto()
                    {
                        Client = name,
                        Amount = amount.ToString(),
                        ExpirationDate = dueDate,
                        IssueDate = new DateTime(),
                        MayoralPayment = mayoral,
                        ReconnectionPayment = reconnection,
                        Company = company,
                        BP = values.CashPointOpenItemSummary?.PartyReference?.InternalID
                    };
                }
            }
            catch (HttpRequestException e)
            {

            }

            return result;
        }
        #endregion

        #region "Cache"
        protected void SaveToken(string key, string token)
        {
            if (_memory != null)
            {
                // Almacena el token en la caché con una expiración de 60 minutos
                var cacheOptions = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromHours(24));

                _memory.Set(key, token, cacheOptions);
            }
        }

        protected string? GetToken(string key)
        {
            string? token = null;
            if (_memory != null)
            {
                _memory.TryGetValue(key, out token);
            }
            return token;
        }
        #endregion

    }
}
