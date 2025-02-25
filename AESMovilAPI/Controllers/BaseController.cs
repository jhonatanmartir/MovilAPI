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
        protected const int INTERNAL_ERROR_500 = StatusCodes.Status500InternalServerError;
        protected const int BAD_GATEWAY_502 = StatusCodes.Status502BadGateway;
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
                case FORBIDDEN_403:     //POST, GET
                    return StatusCode(_statusCode, response);
                case NOT_FOUND_404:     //GET
                    return NotFound(response);
                case INTERNAL_ERROR_500:
                    return StatusCode(_statusCode, response);
                case BAD_GATEWAY_502:
                    return StatusCode(_statusCode, response);
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
        protected async Task<object?> ExecuteGetRequestSAP(string link, bool tokenRequest = false, bool defaultParams = true, bool returnString = false)
        {
            if (_client != null)
            {
                Dictionary<string, string>? queryParams = null;

                if (!tokenRequest && defaultParams)
                {
                    queryParams = new Dictionary<string, string>
                    {
                        { "$expand", "DataSet" },
                        { "$format", "json" }
                    };
                }

                HttpRequestMessage request;
                if (tokenRequest || !defaultParams)
                {
                    // Create HttpRequestMessage
                    request = new HttpRequestMessage(HttpMethod.Get, new Uri(link));

                    // Set custom headers
                    request.Headers.Add(Constants.HEADER_CSFR, Constants.TOKEN_FETCH);
                }
                else
                {
                    // Build the URL with query parameters
                    var urlWithParams = QueryHelpers.AddQueryString(link, queryParams!);
                    // Create HttpRequestMessage
                    request = new HttpRequestMessage(HttpMethod.Get, urlWithParams);
                }

                // Define the username and password for Basic Authentication
                var username = _config.GetValue<string>(Constants.CONF_SAP_USER);
                var password = _config.GetValue<string>(Constants.CONF_SAP_PASSWORD);

                // Create the authentication header value
                var byteArray = Encoding.ASCII.GetBytes($"{username}:{password}");
                var authHeader = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));

                // Set the authorization header
                _client.DefaultRequestHeaders.Authorization = authHeader;

                try
                {
                    // Send the GET request
                    var response = await _client.SendAsync(request);

                    // Ensure the request was successful
                    response.EnsureSuccessStatusCode();

                    // Read the response content as a string
                    var responseContent = await response.Content.ReadAsStringAsync();

                    if (returnString)
                    {
                        return responseContent;
                    }
                    else
                    {
                        try
                        {
                            dynamic responseObject = JsonConvert.DeserializeObject<ExpandoObject>(responseContent)!;

                            if (tokenRequest && response.Headers.TryGetValues(Constants.HEADER_CSFR, out var tokenValue))
                            {
                                string result = tokenValue.ElementAt(0);
                                SaveToken(Constants.RP_TOKEN, result);
                                return result;
                            }

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
                }
                catch (HttpRequestException e)
                {
                    _log.Err(e);
                }
            }

            return null;
        }
        protected async Task<object?> ExecutePostRequestSAP(object postData, string link)
        {
            if (_client != null)
            {
                var jsonContent = JsonConvert.SerializeObject(postData);
                var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                // Define the username and password for Basic Authentication
                var username = _config.GetValue<string>(Constants.CONF_SAP_USER);
                var password = _config.GetValue<string>(Constants.CONF_SAP_PASSWORD);

                // Create the authentication header value
                var byteArray = Encoding.ASCII.GetBytes($"{username}:{password}");

                // Create HttpRequestMessage
                var request = new HttpRequestMessage(HttpMethod.Post, new Uri(link))
                {
                    Content = httpContent
                };
                var authHeader = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));

                // Set the authorization header
                _client.DefaultRequestHeaders.Authorization = authHeader;

                string? token = GetToken(Constants.RP_TOKEN);
                if (string.IsNullOrEmpty(token))
                {
                    token = (string?)await ExecuteGetRequestSAP(link, true, false);
                }

                // Set custom headers
                request.Headers.Add(Constants.HEADER_CSFR, token);

                try
                {
                    // Send the GET request
                    var response = await _client.SendAsync(request);
                    try
                    {
                        // Ensure the request was successful
                        response.EnsureSuccessStatusCode();
                    }
                    catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Forbidden)     //Falta header token
                    {
                        token = GetToken(Constants.RP_TOKEN);
                        if (string.IsNullOrEmpty(token))
                        {
                            token = (string?)await ExecuteGetRequestSAP(link, true, false);
                        }

                        // Set headers token
                        if (response.Headers.Contains(Constants.HEADER_CSFR))
                        {
                            response.Headers.Remove(Constants.HEADER_CSFR);
                        }

                        request.Headers.Add(Constants.HEADER_CSFR, token);

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
                    _statusCode = INTERNAL_ERROR_500;
                    _log.Err(e);
                }
            }

            return null;
        }
        protected async Task<object?> ExecuteGetRequest(string url, bool auth = true, string? token = "", bool bearer = true, bool returnJson = true)
        {
            string result = string.Empty;

            if (_client != null)
            {
                // Create HttpRequestMessage
                var request = new HttpRequestMessage(HttpMethod.Get, new Uri(url));

                if (!bearer)
                {
                    if (string.IsNullOrEmpty(token))
                    {
                        byte[] textoBytes = Encoding.UTF8.GetBytes($"{_config.GetValue<string>(Constants.CONF_SAP_USER)}:{_config.GetValue<string>(Constants.CONF_SAP_PASSWORD)}");
                        token = Convert.ToBase64String(textoBytes);
                    }
                }

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
                    // Send the GET request
                    var response = await _client.SendAsync(request);

                    // Ensure the request was successful
                    response.EnsureSuccessStatusCode();

                    // Read the response content as a string
                    var responseContent = await response.Content.ReadAsStringAsync();
                    if (returnJson)
                    {
                        return JsonConvert.DeserializeObject<ExpandoObject>(responseContent)!;
                    }
                    else
                    {
                        return responseContent;
                    }

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
        /// <summary>
        /// Consulta de saldo en RealPayment SAP
        /// </summary>
        /// <param name="nc">Cuenta Contrato para consultar</param>
        /// <returns>Detalle del saldo</returns>
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
                string endpoint = ApiEndpoint.GetSAPBalance;
                CashPointOpenItemSummaryByElementsResponseMessage values = (CashPointOpenItemSummaryByElementsResponseMessage)await ExecutePostRequestSAP(data, endpoint);

                if (values != null)
                {
                    bool mayoral = false;
                    bool reconnection = false;
                    decimal amount = 0;
                    DateTime dueDate = DateTime.Now;
                    string company = "";
                    string name = "";
                    long documentNumber = 0;
                    decimal mayoralAmount = 0;
                    decimal reconnectionAmount = 0;

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
                                    mayoralAmount += item.OpenAmount.Value;
                                }

                                if (item.OpenItemTransactionDescription.Value == "RECO")
                                {
                                    reconnection = true;
                                    reconnectionAmount += item.OpenAmount.Value;
                                }

                                dueDate = item.DueDate;
                                company = item.PaymentFormID.Split("|")[0];
                                name = item.PaymentFormID.Split("|")[1];
                                documentNumber = string.IsNullOrEmpty(item.InvoiceID) ? 0 : long.Parse(item.InvoiceID);
                            }
                            catch (Exception ex)
                            {
                                _statusCode = INTERNAL_ERROR_500;
                                _log.Err(ex);
                            }
                        }
                    }

                    result = new BillDto()
                    {
                        Client = name,
                        Amount = amount,
                        ExpirationDate = dueDate,
                        IssueDate = new DateTime(),     // TODO
                        MayoralPayment = mayoral,
                        ReconnectionPayment = reconnection,
                        Company = company,
                        BP = values.CashPointOpenItemSummary?.PartyReference?.InternalID,
                        MayoralAmount = mayoralAmount,
                        ReconnectionAmount = reconnectionAmount,
                        DocumentNumber = documentNumber
                    };
                }
            }
            catch (HttpRequestException e)
            {
                _log.Err(e);
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

        protected void SaveCache(string key, object value, int life = 2)
        {
            if (_memory != null)
            {
                // Almacena el token en la caché con una expiración de 60 minutos
                var cacheOptions = new MemoryCacheEntryOptions()
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(life)
                };

                _memory.Set(key, value, cacheOptions);
            }
        }

        protected object? GetFromCache(string key)
        {
            object? result = null;
            if (_memory != null)
            {
                _memory.TryGetValue(key, out result);
            }
            return result;
        }
        #endregion

    }
}
