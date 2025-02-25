using AESMovilAPI.DTOs;
using AESMovilAPI.Responses;
using AESMovilAPI.Services;
using AESMovilAPI.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using System.Text;

namespace AESMovilAPI.Controllers
{
    [Route("api/v1/[controller]")]
    public class ReplacementController : BaseController<ReplacementController>
    {
        public ReplacementController(IConfiguration config, LoggerService<ReplacementController> logger, IHttpClientFactory httpClientFactory, IMemoryCache cache) : base(config, logger, httpClientFactory, cache)
        {
        }

        /// <summary>
        /// Obtener links mediante Cuenta Contrato para descargar PDF de factura y DTE certificado por el Ministerio de Hacienda
        /// </summary>
        /// <param name="id">Cuenta contrato</param>
        /// <returns>Links para descargar PDF y JSON</returns>
        /// <response code="200">Solicitud completada con éxito.</response>
        /// <response code="400">Consulta con datos incorrectos.</response>
        /// <response code="401">Error por token de autorización.</response>
        /// <response code="404">No se encontraron datos.</response>
        /// <response code="500">Error inesperado en el servicio. Intente nuevamente en unos minutos.</response>
        /// <response code="502">Servicio dependiente no respondió correctamente.</response>
        /// <response code="503">Servicio no disponible en este momento.</response>
        [HttpGet("{id}")]
        public async Task<IActionResult> Index(string id)
        {
            Response<ReplacementResponse> response = new Response<ReplacementResponse>();

            if (Helper.IsCuentaContrato(id))
            {
                string fromDate = DateTime.Now.AddMonths(-6).ToString("yyyyMMdd");
                string toDate = DateTime.Now.ToString("yyyyMMdd");
                string endpoint = ApiEndpoint.GetSAPBillHistory(id, fromDate, toDate);
                dynamic? result = await ExecuteGetRequestSAP(endpoint);

                if (result != null)
                {
                    dynamic? data = null;
                    try
                    {
                        data = new
                        {
                            values = result.data.DataSet.results,
                            name = result.data.DataSet.results[0].Cliente,
                            address = result.data.DataSet.results[0].DireccionCliente,
                            fee = result.data.DataSet.results[0].TipoTarifa,
                            company = result.data.DataSet.results[0].Sociedad
                        };
                    }
                    catch (Exception ex)
                    {
                        _log.Err(ex);
                        _statusCode = INTERNAL_ERROR_500;
                    }

                    if (data != null)
                    {
                        var sortedList = ((List<dynamic>)data.values).OrderByDescending(obj =>
                        {
                            DateTime dateValue;
                            return DateTime.TryParse(obj.FechaFacturacion, out dateValue) ? dateValue : DateTime.MinValue;
                        }).ToList();

                        string url = $"{Request.Scheme}://{Request.Host}{Request.PathBase}/api/v1/file/";
                        string documentNumber = sortedList.ElementAt(0).NumRecibo;

                        endpoint = ApiEndpoint.GetSAPJson(documentNumber);

                        result = await ExecuteGetRequestSAP(endpoint, false, false, true);
                        string xmlString = Helper.CleanXml(result, "http://www.w3.org/2005/Atom");
                        var entry = Helper.DeserializeXml<Entry>(xmlString)!;

                        if (!string.IsNullOrEmpty(entry.Content.Properties.Json) && documentNumber == entry.Content.Properties.Opbel)
                        {
                            FileInfoDto dteInfo = new FileInfoDto() { Type = "dte", DocumentNumber = documentNumber };
                            FileInfoDto pdfInfo = new FileInfoDto() { Type = "invoice", DocumentNumber = documentNumber };
                            string dteData = JsonConvert.SerializeObject(dteInfo);
                            string pdfData = JsonConvert.SerializeObject(pdfInfo);

                            dteData = AESEncryption.AES.SetEncrypt(dteData, Constants.ENCRYPT_KEY, Constants.SECRECT_KEY_IV);
                            pdfData = AESEncryption.AES.SetEncrypt(pdfData, Constants.ENCRYPT_KEY, Constants.SECRECT_KEY_IV);

                            ReplacementResponse replacementResponse = new ReplacementResponse()
                            {
                                Dte = url + dteData,
                                Pdf = url + pdfData
                            };

                            response.Data = replacementResponse;
                            response.Success = true;
                            _statusCode = OK_200;
                        }
                    }
                }

                if (!response.Success)
                {
                    var url = $"{Request.Scheme}://{Request.Host}{Request.PathBase}/api/v1/search/{id}";
                    string? token = GetToken(Constants.AESMOVIL_BEARER);

                    if (token == null)
                    {
                        token = await GetBearer();
                    }

                    result = await ExecuteGetRequest(url, true, token);

                    if (result is int numero && numero == UNAUTHORIZED_401)
                    {
                        token = await GetBearer();
                        result = await ExecuteGetRequest(url, true, token);
                    }

                    if (result != null)
                    {
                        var nic = result.data[0].nic;

                        url = _config.GetValue<string>("Legacy:Base") + $"/GetHistoricofact/{nic}";
                        result = await ExecuteGetRequest(url, false);

                        if (result != null && result.data != null && result.data.Count > 0)
                        {
                            ReplacementResponse replacementResponse = new ReplacementResponse()
                            {
                                Dte = result.data[0].dte,
                                Pdf = result.data[0].url_recibo
                            };

                            response.Data = replacementResponse;
                            response.Success = true;
                            _statusCode = OK_200;
                        }
                    }
                    else
                    {
                        _statusCode = NOT_FOUND_404;
                    }
                }
            }

            return GetResponse(response);
        }

        /// <summary>
        /// Obtener links mediante Número de documento para descargar PDF de factura y JSON certificado por el Ministerio de Hacienda.
        /// </summary>
        /// <param name="id">Número de documento</param>
        /// <returns>Links para descargar PDF y JSON</returns>
        /// <response code="200">Solicitud completada con éxito.</response>
        /// <response code="400">Consulta con datos incorrectos.</response>
        /// <response code="401">Error por token de autorización.</response>
        /// <response code="404">No se encontraron datos.</response>
        /// <response code="500">Error inesperado en el servicio. Intente nuevamente en unos minutos.</response>
        /// <response code="502">Servicio dependiente no respondió correctamente.</response>
        /// <response code="503">Servicio no disponible en este momento.</response>
        [HttpGet("Document/{id}")]
        public async Task<IActionResult> Document(string id)
        {
            Response<ReplacementResponse> response = new Response<ReplacementResponse>();

            if (!string.IsNullOrEmpty(id))
            {
                string url = $"{Request.Scheme}://{Request.Host}{Request.PathBase}/api/v1/file/";
                string endpoint = ApiEndpoint.GetSAPJson(id);

                dynamic? result = await ExecuteGetRequestSAP(endpoint, false, false, true);
                string xmlString = Helper.CleanXml(result, "http://www.w3.org/2005/Atom");
                var entry = Helper.DeserializeXml<Entry>(xmlString)!;

                if (!string.IsNullOrEmpty(entry.Content.Properties.Json) && id == entry.Content.Properties.Opbel)
                {
                    FileInfoDto dteInfo = new FileInfoDto() { Type = "dte", DocumentNumber = id };
                    FileInfoDto pdfInfo = new FileInfoDto() { Type = "invoice", DocumentNumber = id };
                    string dteData = JsonConvert.SerializeObject(dteInfo);
                    string pdfData = JsonConvert.SerializeObject(pdfInfo);

                    dteData = AESEncryption.AES.SetEncrypt(dteData, Constants.ENCRYPT_KEY, Constants.SECRECT_KEY_IV);
                    pdfData = AESEncryption.AES.SetEncrypt(pdfData, Constants.ENCRYPT_KEY, Constants.SECRECT_KEY_IV);

                    ReplacementResponse replacementResponse = new ReplacementResponse()
                    {
                        Dte = url + dteData,
                        Pdf = url + pdfData
                    };

                    response.Data = replacementResponse;
                    response.Success = true;
                    _statusCode = OK_200;
                }
                else
                {
                    _statusCode = NOT_FOUND_404;
                }

                /*if (!response.Success)
                {
                    url = $"{Request.Scheme}://{Request.Host}{Request.PathBase}/api/v1/search/{id}";
                    string? token = GetToken(Constants.AESMOVIL_BEARER);

                    if (token == null)
                    {
                        token = await GetBearer();
                    }

                    result = await ExecuteGetRequest(url, true, token);

                    if (result is int numero && numero == UNAUTHORIZED_401)
                    {
                        token = await GetBearer();
                        result = await ExecuteGetRequest(url, true, token);
                    }

                    if (result != null)
                    {
                        var nic = result.data[0].nic;

                        url = _config.GetValue<string>("Legacy:Base") + $"/GetHistoricofact/{nic}";
                        result = await ExecuteGetRequest(url, false);

                        if (result != null && result.data != null && result.data.Count > 0)
                        {
                            ReplacementResponse replacementResponse = new ReplacementResponse()
                            {
                                Dte = result.data[0].dte,
                                Pdf = result.data[0].url_recibo
                            };

                            response.Data = replacementResponse;
                            response.Success = true;
                            _statusCode = OK_200;
                        }
                    }
                    else
                    {
                        _statusCode = NOT_FOUND_404;
                    }
                }*/
            }
            else
            {
                _statusCode = BAD_REQUEST_400;
            }

            return GetResponse(response);
        }

        private async Task<string?> GetBearer()
        {
            var url = $"{Request.Scheme}://{Request.Host}{Request.PathBase}/api/v1/auth/login";
            var authorizedUsers = _config.GetSection("Authorized").Get<Dictionary<string, string>>();

            if (authorizedUsers != null && authorizedUsers.Count > 0)
            {
                byte[] textoBytes = Encoding.UTF8.GetBytes(authorizedUsers.ElementAt(0).Key + ":" + authorizedUsers.ElementAt(0).Value);
                string textoBase64 = Convert.ToBase64String(textoBytes);
                var postData = new
                {
                    auth = "Basic " + textoBase64
                };

                dynamic? authResponse = await ExecutePostRequest(postData, url, false);

                if (authResponse != null && authResponse.success)
                {
                    SaveToken(Constants.AESMOVIL_BEARER, authResponse.data);
                    return authResponse.data;
                }
            }

            return null;
        }
    }
}
