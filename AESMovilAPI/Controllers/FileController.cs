using AESMovilAPI.Utilities;
using iText.Kernel.Colors;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Draw;
using iText.Layout;
using iText.Layout.Borders;
using iText.Layout.Element;
using iText.Layout.Properties;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Newtonsoft.Json;
using System.Dynamic;
using System.Net.Http.Headers;
using System.Text;

namespace AESMovilAPI.Controllers
{
    [Route("api/v1/[controller]")]
    public class FileController : BaseController
    {
        private readonly HttpClient _client;
        public FileController(IConfiguration config, HttpClient client) : base(config)
        {
            _client = client;
        }

        [NonAction]
        [HttpGet("Bill/{id}")]
        public IActionResult GetBill(string id)
        {
            return NotFound();
        }

        [NonAction]
        [HttpGet("Dte/{id}")]
        public IActionResult GetDte(string id)
        {
            return NotFound();
        }

        /// <summary>
        /// Obtener reporte PDF de historico de facturación
        /// </summary>
        /// <param name="id">NC del cliente</param>
        /// <param name="fromDate">Desde que fecha se consulta el historico en formato <c>yyyyMMdd</c> por ejemplo <c>20200101</c>. Sino se especifica se tomará 4 años atras, a partir de la fecha actual.</param>
        /// <param name="toDate">Hasta que fecha se consulta el historico en formato <c>yyyyMMdd</c> por ejemplo <c>202400606</c>. Sino se especifica se tomará la fecha actual.</param>
        /// <returns>Archivo PDF</returns>
        /// <response code="200">Correcto.</response>
        /// <response code="400">Parametros incorrectos.</response>
        /// <response code="401">Error por token de autorización.</response>
        /// <response code="404">No se encontró historico de facturación.</response>
        /// <response code="500">Ha ocurrido un error faltal en el servicio.</response>
        /// <response code="502">Incidente en el servicio.</response>
        [HttpGet("BillingHistory/{id}/{fromDate=}/{toDate=}")]
        public async Task<IActionResult> GetBillingHistory(string id, string? fromDate = null, string? toDate = null)
        {
            if (string.IsNullOrEmpty(fromDate) || string.IsNullOrEmpty(fromDate.Trim(',')))
            {
                fromDate = DateTime.Now.AddYears(-4).ToString("yyyyMMdd");
            }

            if (string.IsNullOrEmpty(toDate) || string.IsNullOrEmpty(toDate.Trim(',')))
            {
                toDate = DateTime.Now.ToString("yyyyMMdd");
            }
            dynamic? data = await GetBillingHistoryData(id, fromDate, toDate);
            byte[] fileBytes = BuildBillingHistoryFile(id, fromDate, toDate, data);
            return File(fileBytes, "application/pdf", "BillingHistory-" + id + ".pdf");
        }

        /// <summary>
        /// Obtener reporte PDF de historico de alcaldía
        /// </summary>
        /// <param name="id">NC del cliente</param>
        /// <param name="fromDate">Desde que fecha se consulta el historico en formato <c>yyyyMMdd</c> por ejemplo <c>20200101</c>. Sino se especifica se tomará 4 años atras, a partir de la fecha actual.</param>
        /// <param name="toDate">Hasta que fecha se consulta el historico en formato <c>yyyyMMdd</c> por ejemplo <c>202400606</c>. Sino se especifica se tomará la fecha actual.</param>
        /// <returns>Archivo PDF</returns>
        /// <response code="200">Correcto.</response>
        /// <response code="400">Parametros incorrectos.</response>
        /// <response code="401">Error por token de autorización.</response>
        /// <response code="404">No se encontró historico de alcaldía.</response>
        /// <response code="500">Ha ocurrido un error faltal en el servicio.</response>
        /// <response code="502">Incidente en el servicio.</response>
        [HttpGet("MayoralHistory/{id}/{fromDate=}/{toDate=}")]
        public async Task<IActionResult> GetMayoralHistory(string id, string? fromDate = null, string? toDate = null)
        {
            if (string.IsNullOrEmpty(fromDate) || string.IsNullOrEmpty(fromDate.Trim(',')))
            {
                fromDate = DateTime.Now.AddYears(-4).ToString("yyyyMMdd");
            }

            if (string.IsNullOrEmpty(toDate) || string.IsNullOrEmpty(toDate.Trim(',')))
            {
                toDate = DateTime.Now.ToString("yyyyMMdd");
            }

            dynamic? data = await GetMayoralHistoryData(id, fromDate, toDate);
            byte[] fileBytes = BuildMayoralHistoryFile(id, fromDate, toDate, data);
            return File(fileBytes, "application/pdf", "MayoralHistory-" + id + ".pdf");
        }

        #region "MayoralHistory"
        private async Task<object?> GetMayoralHistoryData(string nc, string fromDate, string toDate)
        {
            string baseUrl = "https://aes-cf-gcp-1kg8o7mu.it-cpi017-rt.cfapps.us30.hana.ondemand.com/gw/odata/SAP/";
            string mandante = "CCG160";
            string link = baseUrl + "CIS_" + mandante + "_ACC_GETHISTORICOCARGOSALCALDIA_AZUREAPPSSERVICES_TO_SAPCIS;v=1/GetHistoricoAlcaldiaSet(Nic='" + nc + "',Fechainicio='" + fromDate + "',Fechafin='" + toDate + "')";
            var responseContent = "";
            dynamic? result = null;

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

                if (string.IsNullOrEmpty(responseObject.d.Errorcode) || responseObject.d.Errorcode == "0")
                {
                    result = responseObject.d.DataSet;
                }
            }
            catch (HttpRequestException e)
            {

            }

            return result;
        }

        private byte[]? BuildMayoralHistoryFile(string nc, string fromDate, string toDate, dynamic data)
        {
            if (data != null)
            {
                using (MemoryStream msTemp = new MemoryStream())
                {
                    using (PdfWriter pdfWriter = new PdfWriter(msTemp))
                    {
                        using (PdfDocument pdf = new PdfDocument(pdfWriter))
                        {
                            Document document = new Document(pdf);

                            // Header
                            Paragraph header = new Paragraph("HISTORICO DE ALCALDÍA")
                                .SetTextAlignment(TextAlignment.CENTER)
                                .SetFontSize(14)
                                .SetBold();
                            // New line
                            Paragraph newline = new Paragraph(new Text("\n"));

                            document.Add(header);
                            document.Add(newline);

                            // Add sub-header
                            Paragraph subheader1 = new Paragraph("NC: " + nc)
                               .SetTextAlignment(TextAlignment.CENTER)
                               .SetFontSize(12);
                            Paragraph subheader = new Paragraph("Desde " + Helper.ParseStrDateMonth(fromDate) + " hasta " + Helper.ParseStrDateMonth(toDate))
                               .SetTextAlignment(TextAlignment.CENTER)
                               .SetFontSize(12);
                            document.Add(subheader1);
                            document.Add(subheader);

                            // Line separator
                            LineSeparator ls = new LineSeparator(new SolidLine());
                            document.Add(ls);

                            // Table
                            Table table = new Table(7, false);
                            Cell cell11 = new Cell(1, 1)
                                .SetBorder(Border.NO_BORDER)
                                //.SetBackgroundColor(ColorConstants.LIGHT_GRAY)
                                .SetTextAlignment(TextAlignment.CENTER)
                                .Add(new Paragraph("CARGO").SetBold().SetFontSize(9));
                            Cell cell12 = new Cell(1, 1)
                                .SetBorder(Border.NO_BORDER)
                               //.SetBackgroundColor(ColorConstants.LIGHT_GRAY)
                               .SetTextAlignment(TextAlignment.CENTER)
                               .Add(new Paragraph("ESTADO").SetBold().SetFontSize(9));
                            Cell cell13 = new Cell(1, 1)
                                .SetBorder(Border.NO_BORDER)
                               //.SetBackgroundColor(ColorConstants.LIGHT_GRAY)
                               .SetTextAlignment(TextAlignment.CENTER)
                               .Add(new Paragraph("FECHA FACTURADO").SetBold().SetFontSize(9));
                            Cell cell14 = new Cell(1, 1)
                                .SetBorder(Border.NO_BORDER)
                               //.SetBackgroundColor(ColorConstants.LIGHT_GRAY)
                               .SetTextAlignment(TextAlignment.CENTER)
                               .Add(new Paragraph("FECHA COBRO").SetBold().SetFontSize(9));
                            Cell cell15 = new Cell(1, 1)
                                .SetBorder(Border.NO_BORDER)
                               //.SetBackgroundColor(ColorConstants.LIGHT_GRAY)
                               .SetTextAlignment(TextAlignment.CENTER)
                               .Add(new Paragraph("IMPORTE").SetBold().SetFontSize(9));
                            Cell cell16 = new Cell(1, 1)
                                .SetBorder(Border.NO_BORDER)
                               //.SetBackgroundColor(ColorConstants.LIGHT_GRAY)
                               .SetTextAlignment(TextAlignment.CENTER)
                               .Add(new Paragraph("ALCALDÍA").SetBold().SetFontSize(9));
                            Cell cell17 = new Cell(1, 1)
                                .SetBorder(Border.NO_BORDER)
                               //.SetBackgroundColor(ColorConstants.LIGHT_GRAY)
                               .SetTextAlignment(TextAlignment.CENTER)
                               .Add(new Paragraph("CUENTA").SetBold().SetFontSize(9));

                            table.AddCell(cell11);
                            table.AddCell(cell12);
                            table.AddCell(cell13);
                            table.AddCell(cell14);
                            table.AddCell(cell15);
                            table.AddCell(cell16);
                            table.AddCell(cell17);

                            foreach (var item in data.results)
                            {
                                Cell cell1 = new Cell(1, 1)
                                    .SetBorder(Border.NO_BORDER)
                                    .SetTextAlignment(TextAlignment.LEFT)
                                    .Add(new Paragraph(item.CargoVario).SetFontSize(9));
                                Cell cell2 = new Cell(1, 1)
                                    .SetBorder(Border.NO_BORDER)
                                    .SetTextAlignment(TextAlignment.LEFT)
                                    .Add(new Paragraph(item.Estado).SetFontSize(9));
                                Cell cell3 = new Cell(1, 1)
                                    .SetBorder(Border.NO_BORDER)
                                    .SetTextAlignment(TextAlignment.LEFT)
                                    .Add(new Paragraph(Helper.ParseStrDate(item.FechaFacturado)).SetFontSize(9));
                                Cell cell4 = new Cell(1, 1)
                                    .SetBorder(Border.NO_BORDER)
                                    .SetTextAlignment(TextAlignment.LEFT)
                                    .Add(new Paragraph(Helper.ParseStrDate(item.FechaCobro)).SetFontSize(9));
                                Cell cell5 = new Cell(1, 1)
                                    .SetBorder(Border.NO_BORDER)
                                    .SetTextAlignment(TextAlignment.LEFT)
                                    .Add(new Paragraph("$" + item.Monto.Trim()).SetFontSize(9));
                                Cell cell6 = new Cell(1, 1)
                                    .SetBorder(Border.NO_BORDER)
                                    .SetTextAlignment(TextAlignment.LEFT)
                                    .Add(new Paragraph(item.Alcaldia).SetFontSize(9));
                                Cell cell7 = new Cell(1, 1)
                                    .SetBorder(Border.NO_BORDER)
                                    .SetTextAlignment(TextAlignment.LEFT)
                                    .Add(new Paragraph(item.Cuenta).SetFontSize(9));

                                table.AddCell(cell1);
                                table.AddCell(cell2);
                                table.AddCell(cell3);
                                table.AddCell(cell4);
                                table.AddCell(cell5);
                                table.AddCell(cell6);
                                table.AddCell(cell7);
                            }

                            document.Add(newline);
                            document.Add(table);

                            document.Close();
                            return msTemp.ToArray();
                        }
                    }
                }
            }
            return null;
        }
        #endregion

        #region "BillingHistory"
        private async Task<object?> GetBillingHistoryData(string nc, string fromDate, string toDate)
        {
            string baseUrl = "https://aes-cf-gcp-1kg8o7mu.it-cpi017-rt.cfapps.us30.hana.ondemand.com/gw/odata/SAP/";
            string mandante = "CCG160";
            string link = baseUrl + "CIS_" + mandante + "_BIL_BILLIMAGEPREVIEWES_AZUREAPPSERVICES_TO_SAPCIS;v=1/InvHistSummarySet(Nic='" + nc + "',Ab='" + fromDate + "',Bis='" + toDate + "')";

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
                        values = responseObject.d.DataSet.results,
                        name = responseObject.d.DataSet.results[0].Cliente,
                        address = responseObject.d.DataSet.results[0].DireccionCliente,
                        fee = responseObject.d.DataSet.results[0].TipoTarifa
                    };
                }
            }
            catch (HttpRequestException e)
            {

            }

            return null;
        }

        private byte[]? BuildBillingHistoryFile(string nc, string fromDate, string toDate, dynamic data)
        {
            if (data != null)
            {
                using (MemoryStream msTemp = new MemoryStream())
                {
                    using (PdfWriter pdfWriter = new PdfWriter(msTemp))
                    {
                        using (PdfDocument pdf = new PdfDocument(pdfWriter))
                        {
                            Document document = new Document(pdf);

                            // Header
                            Paragraph header = new Paragraph("HISTORICO DE FACTURACIÓN")
                                .SetTextAlignment(TextAlignment.CENTER)
                                .SetFontSize(14)
                                .SetBold();
                            // New line
                            Paragraph newline = new Paragraph(new Text("\n"));

                            document.Add(header);
                            document.Add(newline);

                            // Add sub-header
                            Paragraph subheader1 = new Paragraph("NC:       " + nc)
                               .SetTextAlignment(TextAlignment.CENTER)
                               .SetFontSize(12);
                            Paragraph subheader = new Paragraph("Desde " + Helper.ParseStrDateMonth(fromDate) + " hasta " + Helper.ParseStrDateMonth(toDate))
                               .SetTextAlignment(TextAlignment.CENTER)
                               .SetFontSize(12);
                            document.Add(subheader1);
                            document.Add(subheader);

                            // Line separator
                            //LineSeparator ls = new LineSeparator(new SolidLine());
                            //document.Add(ls);

                            // Table
                            Table table = new Table(7, false);
                            Cell cell11 = new Cell(1, 1)
                                .SetBorder(Border.NO_BORDER)
                                .SetBorderBottom(new SolidBorder(ColorConstants.GRAY, 1))
                                //.SetBackgroundColor(ColorConstants.LIGHT_GRAY)
                                .SetTextAlignment(TextAlignment.CENTER)
                                .Add(new Paragraph("NUMERO DE FACTURA").SetBold().SetFontSize(7));
                            Cell cell12 = new Cell(1, 1)
                                .SetBorder(Border.NO_BORDER)
                                .SetBorderBottom(new SolidBorder(ColorConstants.GRAY, 1))
                               //.SetBackgroundColor(ColorConstants.LIGHT_GRAY)
                               .SetTextAlignment(TextAlignment.CENTER)
                               .Add(new Paragraph("FECHA DE FACTURACIÓN").SetBold().SetFontSize(7));
                            Cell cell13 = new Cell(1, 1)
                                .SetBorder(Border.NO_BORDER)
                                .SetBorderBottom(new SolidBorder(ColorConstants.GRAY, 1))
                               //.SetBackgroundColor(ColorConstants.LIGHT_GRAY)
                               .SetTextAlignment(TextAlignment.CENTER)
                               .Add(new Paragraph("MONTO FACTURADO").SetBold().SetFontSize(7));
                            //Cell cell14 = new Cell(1, 1)
                            //    .SetBorder(Border.NO_BORDER)
                            //    .SetBorderBottom(new SolidBorder(ColorConstants.GRAY, 1))
                            //   //.SetBackgroundColor(ColorConstants.LIGHT_GRAY)
                            //   .SetTextAlignment(TextAlignment.CENTER)
                            //   .Add(new Paragraph("FECHA DE VENCIMIENTO").SetBold().SetFontSize(8));
                            Cell cell15 = new Cell(1, 1)
                                .SetBorder(Border.NO_BORDER)
                                .SetBorderBottom(new SolidBorder(ColorConstants.GRAY, 1))
                               //.SetBackgroundColor(ColorConstants.LIGHT_GRAY)
                               .SetTextAlignment(TextAlignment.CENTER)
                               .Add(new Paragraph("MONTO CANCELADO").SetBold().SetFontSize(7));
                            Cell cell16 = new Cell(1, 1)
                                .SetBorder(Border.NO_BORDER)
                                .SetBorderBottom(new SolidBorder(ColorConstants.GRAY, 1))
                               //.SetBackgroundColor(ColorConstants.LIGHT_GRAY)
                               .SetTextAlignment(TextAlignment.CENTER)
                               .Add(new Paragraph("FECHA DE PAGO").SetBold().SetFontSize(7));
                            Cell cell17 = new Cell(1, 1)
                                .SetBorder(Border.NO_BORDER)
                                .SetBorderBottom(new SolidBorder(ColorConstants.GRAY, 1))
                               //.SetBackgroundColor(ColorConstants.LIGHT_GRAY)
                               .SetTextAlignment(TextAlignment.LEFT)
                               .Add(new Paragraph("ESTADO").SetBold().SetFontSize(8));
                            Cell cell18 = new Cell(1, 1)
                                .SetBorder(Border.NO_BORDER)
                                .SetBorderBottom(new SolidBorder(ColorConstants.GRAY, 1))
                               //.SetBackgroundColor(ColorConstants.LIGHT_GRAY)
                               .SetTextAlignment(TextAlignment.LEFT)
                               .Add(new Paragraph("LUGAR DE PAGO").SetBold().SetFontSize(7));


                            table.AddCell(cell11);
                            table.AddCell(cell12);
                            table.AddCell(cell13);
                            //table.AddCell(cell14);
                            table.AddCell(cell15);
                            table.AddCell(cell16);
                            table.AddCell(cell17);
                            table.AddCell(cell18);

                            foreach (var item in data.values)
                            {
                                Cell cell1 = new Cell(1, 1)
                                    .SetBorder(Border.NO_BORDER)
                                    .SetTextAlignment(TextAlignment.CENTER)
                                    .Add(new Paragraph(item.NumRecibo).SetFontSize(7));
                                Cell cell2 = new Cell(1, 1)
                                    .SetBorder(Border.NO_BORDER)
                                    .SetTextAlignment(TextAlignment.CENTER)
                                    .Add(new Paragraph(item.FechaFacturacion).SetFontSize(7));
                                Cell cell3 = new Cell(1, 1)
                                    .SetBorder(Border.NO_BORDER)
                                    .SetTextAlignment(TextAlignment.RIGHT)
                                    .Add(new Paragraph("$ " + item.ImpFact.Trim()).SetFontSize(7));
                                //Cell cell4 = new Cell(1, 1)
                                //    .SetBorder(Border.NO_BORDER)
                                //    .SetTextAlignment(TextAlignment.LEFT)
                                //    .Add(new Paragraph(Helper.ParseStrDate(item.FechaCobro)).SetFontSize(8));
                                Cell cell5 = new Cell(1, 1)
                                    .SetBorder(Border.NO_BORDER)
                                    .SetTextAlignment(TextAlignment.RIGHT)
                                    .Add(new Paragraph("$ " + item.ImpFactCanc.Trim()).SetFontSize(7));
                                Cell cell6 = new Cell(1, 1)
                                    .SetBorder(Border.NO_BORDER)
                                    .SetTextAlignment(TextAlignment.CENTER)
                                    .Add(new Paragraph(item.FechaPago).SetFontSize(7));
                                Cell cell7 = new Cell(1, 1)
                                .SetBorder(Border.NO_BORDER)
                                    .SetTextAlignment(TextAlignment.LEFT)
                                    .Add(new Paragraph(item.Status).SetFontSize(7));
                                Cell cell8 = new Cell(1, 1)
                                    .SetBorder(Border.NO_BORDER)
                                    .SetTextAlignment(TextAlignment.LEFT)
                                    .Add(new Paragraph(item.LugarPago).SetFontSize(7));

                                table.AddCell(cell1);
                                table.AddCell(cell2);
                                table.AddCell(cell3);
                                //table.AddCell(cell4);
                                table.AddCell(cell5);
                                table.AddCell(cell6);
                                table.AddCell(cell7);
                                table.AddCell(cell8);
                            }

                            //document.Add(newline);
                            document.Add(table);

                            document.Close();
                            return msTemp.ToArray();
                        }
                    }
                }
            }
            return null;
        }
        #endregion
    }
}