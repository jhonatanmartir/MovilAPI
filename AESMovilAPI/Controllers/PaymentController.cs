using AESMovilAPI.DTOs;
using AESMovilAPI.Responses;
using AESMovilAPI.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Newtonsoft.Json;
using System.Dynamic;
using System.Net.Http.Headers;
using System.Text;

namespace AESMovilAPI.Controllers
{
    [Route("api/v1/[controller]")]
    public class PaymentController : BaseController
    {
        private readonly HttpClient _client;
        public PaymentController(IConfiguration config, HttpClient httpClient) : base(config)
        {
            _client = httpClient;
        }

        /// <summary>
        /// Generar un link de pago.
        /// </summary>
        /// <param name="data">Objeto que representa <see cref="PaymentDto">PaymentDto</see> para crear links de pago.</param>
        /// <returns>Link de pago.</returns>
        /// <response code="201">Link creado.</response>
        /// <response code="400">Datos incompletos.</response>
        /// <response code="401">Error por token de autorización.</response>
        /// <response code="404">No se genero el link.</response>
        /// <response code="500">Ha ocurrido un error faltal en el servicio.</response>
        /// <response code="502">Incidente en el servicio.</response>
        [HttpPost]
        public async Task<IActionResult> Create(PaymentDto data)
        {
            Response<string> response = new Response<string>();

            if (data != null && ModelState.IsValid)
            {
                _statusCode = NOT_FOUND_404;
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
            string token = "mGZl9TQmpIxm9MUiLODM1lwzNacRVV846UBqv1mq/qPL2NasUbhhQz6lkIB1TB3dq8lN8Idq/Dv9yGtYkdGvLA==";
            BillDto? bill = await GetInvoiceData(nc);

            if (bill != null)
            {
                string usuarioOperacion = GetUsuarioOperacionPayway(bill.Company);
                string colectorId = GetcolectorIdPayway(bill.Company);
                var postData = new
                {
                    token = token,                                                          // Req. Const
                    usuarioOperacion = usuarioOperacion,                                    // Req. dinamic
                    colectorId = colectorId,                                                // Req. dinamic
                    clienteNombre = bill.Client,                                            // Dinamic
                    concepto = "Pago de factura " + bill.IssueDate.ToString("MMMM/yyyy"),   // Dinamic
                    monto = bill.Amount,                                                    // Dinamic
                    datosAuxiliares = new
                    {
                        datoAuxiliar1 = nc,                                                 // NC
                        datoAuxiliar2 = bill.ExpirationDate.ToString("yyyyMMdd"),           // Vencimiento YYYYMMDD
                        datoAuxiliar3 = bill.MayoralPayment ? "1" : "0",                    // Paga alcaldia = 1, else 0
                        datoAuxiliar4 = bill.ReconnectionPayment ? "1" : "0",                // Paga recon = 1, else 0
                        datoAuxiliar5 = ""                // Paga recon = 1, else 0
                    },
                };

                var jsonContent = JsonConvert.SerializeObject(postData);
                using var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                try
                {
                    // Send the POST request
                    var response = await _client.PostAsync("https://test.payway.sv/web-payway-sv/paywayone/api/rest/links", httpContent);

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
                var user = "e45e8561e4a694e369bd78267bd5a828";
                var pwd = "ca557e96b6ef72d973ed99a09b68a797";
                var postData = new
                {
                    ern = bill.DocumentNumberId,        // TODO confirm value
                    amount = bill.Amount,
                    currency = "USD",
                    extended_expiration = false,
                    details = new List<object>
                    {
                        new
                        {
                            quantity = 1,
                            description = "Pago de factura " + bill.IssueDate.ToString("MMMM/yyyy"),
                            price = bill.Amount
                        }
                    }
                };

                var jsonContent = JsonConvert.SerializeObject(postData);
                using var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                try
                {
                    // Create the authentication header value
                    var byteArray = Encoding.ASCII.GetBytes($"{user}:{pwd}");
                    var authHeader = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
                    // Set the authorization header
                    _client.DefaultRequestHeaders.Authorization = authHeader;

                    // Send the POST request
                    var response = await _client.PostAsync("https://sandbox-connect.pagadito.com/api/v2/exec-trans", httpContent);

                    // Ensure the request was successful
                    response.EnsureSuccessStatusCode();

                    // Read the response content as a string
                    var responseContent = await response.Content.ReadAsStringAsync();
                    dynamic responseObject = JsonConvert.DeserializeObject<ExpandoObject>(responseContent)!;

                    if (responseObject.Code == "00")
                    {
                        result = responseObject.Data.url;
                    }
                }
                catch (HttpRequestException e)
                {

                }
            }

            return result;
        }

        private async Task<BillDto?> GetInvoiceData(string nc)
        {
            string baseUrl = "https://aes-cf-gcp-1kg8o7mu.it-cpi017-rt.cfapps.us30.hana.ondemand.com/gw/odata/SAP/";
            string mandante = "CCG160";
            string link = baseUrl + "CIS_" + mandante + "_BIL_LASTACCOUNTBALANCE_AZUREAPPSERVICES_TO_SAPCIS;v=1/PendingDebtDetailsSet('" + nc + "')";
            var responseContent = "";
            BillDto? result = null;

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
                responseContent = await response.Content.ReadAsStringAsync();
                dynamic responseObject = JsonConvert.DeserializeObject<ExpandoObject>(responseContent)!;

                if (responseObject.d.MessageType == "0")
                {
                    result = new BillDto()
                    {
                        Client = responseObject.d.DataSet.results[0].Cliente,
                        Amount = responseObject.d.DataSet.results[0].SaldoPagar.Trim(),
                        ExpirationDate = Helper.ParseDate(responseObject.d.DataSet.results[0].FVencimiento),
                        IssueDate = Helper.ParseDate(responseObject.d.DataSet.results[0].F_emision, "yyyyMMdd"),
                        MayoralPayment = Decimal.Parse(responseObject.d.DataSet.results[0].Alcaldia) > 0 ? true : false,
                        ReconnectionPayment = Decimal.Parse(responseObject.d.DataSet.results[0].Reconexion) > 0 ? true : false,
                        Company = responseObject.d.DataSet.results[0].Empresa,
                        DocumentNumberId = responseObject.d.DataSet.results[0].IdCobro
                    };
                }
            }
            catch (HttpRequestException e)
            {

            }

            return result;
        }

        private string GetUsuarioOperacionPayway(string company)
        {
            string value = string.Empty;
            switch (company)
            {
                case Constants.SV10_CAESS:
                    value = "PAGOS.LINK.CAESS";
                    break;
                case Constants.SV20_EEO:
                    value = "PAGOS.LINK.EEO";
                    break;
                case Constants.SV30_DEUSEM:
                    value = "PAGOS.LINK.DEUSEM";
                    break;
                case Constants.SV40_CLESA:
                    value = "PAGOS.LINK.CLESA";
                    break;
            }
            return value;
        }
        private string GetcolectorIdPayway(string company)
        {
            string value = string.Empty;
            switch (company)
            {
                case Constants.SV10_CAESS:
                    value = "384";
                    break;
                case Constants.SV20_EEO:
                    value = "386";
                    break;
                case Constants.SV30_DEUSEM:
                    value = "387";
                    break;
                case Constants.SV40_CLESA:
                    value = "385";
                    break;
            }
            return value;
        }
    }
}
