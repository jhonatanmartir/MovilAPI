﻿using AESMovilAPI.DTOs;
using AESMovilAPI.Responses;
using AESMovilAPI.Services;
using AESMovilAPI.Utilities;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Dynamic;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Text;

namespace AESMovilAPI.Controllers
{
    [Route("api/v1/[controller]")]
    public class PaymentController : BaseController<PaymentController>
    {
        public PaymentController(IConfiguration config, LoggerService<PaymentController> logger, IHttpClientFactory httpClientFactory) : base(config, logger, httpClientFactory)
        {

        }

        /// <summary>
        /// Generar un link de pago.
        /// </summary>
        /// <param name="data">Objeto que representa <see cref="Payment">PaymentDto</see> para crear links de pago.</param>
        /// <returns>Link de pago.</returns>
        /// <response code="201">Link creado.</response>
        /// <response code="400">Datos incorrectos.</response>
        /// <response code="401">Error por token de autorización.</response>
        /// <response code="404">No se generó link, no hay deuda que pagar.</response>
        /// <response code="500">Ha ocurrido un error faltal en el servicio.</response>
        /// <response code="502">Incidente en el servicio.</response>
        [HttpPost]
        public async Task<IActionResult> Create(Payment data)
        {
            Response<string> response = new Response<string>();

            if (data != null && ModelState.IsValid)
            {
                _statusCode = BAD_REQUEST_400;
                switch (data.Collector.ToUpper())
                {
                    case "PAGADITO":
                        response.Data = await GetPagaditoLink(data.NC.ToString());
                        break;
                    case "PAYWAY":
                        response.Data = await GetPaywayLink(data.NC.ToString());
                        break;
                    default: break;
                }
                _statusCode = string.IsNullOrEmpty(response.Data) ? NOT_FOUND_404 : CREATED_201;
            }

            return GetResponse(response);
        }

        private async Task<string> GetPaywayLink(string nc)
        {
            string result = string.Empty;
            string token = _config.GetValue<string>(Constants.CONF_PAYWAY_TOKEN);

            BillDto? bill = await GetInvoiceData(nc);

            if (bill != null)
            {
                string usuarioOperacion = GetUsuarioOperacionPayway(bill.Company);
                string colectorId = GetColectorIdPayway(bill.Company);
                var postData = new
                {
                    token = token,                                                          // Req. Const
                    usuarioOperacion = usuarioOperacion,                                    // Req. dinamic
                    colectorId = colectorId,                                                // Req. dinamic
                    clienteNombre = bill.Client,                                            // Dinamic
                    concepto = "Pago de factura",                                           // Dinamic
                    monto = bill.Amount.ToString(),                                         // Dinamic
                    datosAuxiliares = new
                    {
                        datoAuxiliar1 = nc,                                                 // NC
                        datoAuxiliar2 = bill.ExpirationDate.ToString("yyyyMMdd"),           // Vencimiento YYYYMMDD
                        datoAuxiliar3 = bill.MayoralPayment ? "1" : "0",                    // Paga alcaldia = 1, else 0
                        datoAuxiliar4 = bill.ReconnectionPayment ? "1" : "0",               // Paga recon = 1, else 0
                        datoAuxiliar5 = bill.BP                                             // BP
                    },
                };

                var jsonContent = JsonConvert.SerializeObject(postData);
                using var httpContent = new StringContent(jsonContent, Encoding.UTF8, MediaTypeNames.Application.Json);

                try
                {
                    // Send the POST request
                    var response = await _client.PostAsync(_config.GetValue<string>(Constants.CONF_PAYWAY_ENDPOINT), httpContent);

                    // Ensure the request was successful
                    response.EnsureSuccessStatusCode();

                    // Read the response content as a string
                    var responseContent = await response.Content.ReadAsStringAsync();
                    dynamic responseObject = JsonConvert.DeserializeObject<ExpandoObject>(responseContent)!;

                    if (responseObject.returnCode == "00")
                    {
                        result = responseObject.linkurl;
                    }
                }
                catch (HttpRequestException e)
                {
                    _log.Err(e);
                }
            }

            return result;
        }

        private async Task<string> GetPagaditoLink(string nc)
        {
            string result = string.Empty;
            BillDto? bill = await GetInvoiceData(nc);

            if (bill != null)
            {
                var user = _config.GetValue<string>(Constants.CONF_PAGADITO_USER);
                var pwd = _config.GetValue<string>(Constants.CONF_PAGADITO_PASSWORD);
                var postData = new
                {
                    ern = nc,
                    amount = bill.Amount,
                    details = new List<object>
                    {
                        new
                        {
                            quantity = 1,
                            description = "Pago de factura",
                            price = bill.Amount.ToString(),
                            url_product = ""
                        }
                    }
                };

                var jsonContent = JsonConvert.SerializeObject(postData);
                using var httpContent = new StringContent(jsonContent, Encoding.UTF8, MediaTypeNames.Application.Json);

                try
                {
                    // Create the authentication header value
                    var byteArray = Encoding.ASCII.GetBytes($"{user}:{pwd}");
                    var authHeader = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
                    // Set the authorization header
                    _client.DefaultRequestHeaders.Authorization = authHeader;

                    // Send the POST request
                    var response = await _client.PostAsync(_config.GetValue<string>(Constants.CONF_PAGADITO_ENDPOINT), httpContent);

                    // Ensure the request was successful
                    response.EnsureSuccessStatusCode();

                    // Read the response content as a string
                    var responseContent = await response.Content.ReadAsStringAsync();
                    dynamic responseObject = JsonConvert.DeserializeObject<ExpandoObject>(responseContent)!;

                    if (responseObject.code == "00" || responseObject.code == "PG1002")
                    {
                        result = responseObject.data.url;
                    }
                }
                catch (HttpRequestException e)
                {

                }
            }

            return result;
        }

        private string GetUsuarioOperacionPayway(string company)
        {
            string value = string.Empty;
            switch (company)
            {
                case Constants.SV10_CAESS:
                case Constants.SV10_CAESS_CODE:
                    value = _config.GetValue<string>(Constants.CONF_PAYWAY_USER_CAESS);
                    break;
                case Constants.SV20_DEUSEM:
                case Constants.SV20_DEUSEM_CODE:
                    value = _config.GetValue<string>(Constants.CONF_PAYWAY_USER_DEUSEM);
                    break;
                case Constants.SV30_EEO:
                case Constants.SV30_EEO_CODE:
                    value = _config.GetValue<string>(Constants.CONF_PAYWAY_USER_EEO);
                    break;
                case Constants.SV40_CLESA:
                case Constants.SV40_CLESA_CODE:
                    value = _config.GetValue<string>(Constants.CONF_PAYWAY_USER_CLESA);
                    break;
            }
            return value;
        }
        private string GetColectorIdPayway(string company)
        {
            string value = string.Empty;
            switch (company)
            {
                case Constants.SV10_CAESS:
                case Constants.SV10_CAESS_CODE:
                    value = _config.GetValue<string>(Constants.CONF_PAYWAY_ID_CAESS);
                    break;
                case Constants.SV20_DEUSEM:
                case Constants.SV20_DEUSEM_CODE:
                    value = _config.GetValue<string>(Constants.CONF_PAYWAY_ID_DEUSEM);
                    break;
                case Constants.SV30_EEO:
                case Constants.SV30_EEO_CODE:
                    value = _config.GetValue<string>(Constants.CONF_PAYWAY_ID_EEO);
                    break;
                case Constants.SV40_CLESA:
                case Constants.SV40_CLESA_CODE:
                    value = _config.GetValue<string>(Constants.CONF_PAYWAY_ID_CLESA);
                    break;
            }
            return value;
        }
    }
}
