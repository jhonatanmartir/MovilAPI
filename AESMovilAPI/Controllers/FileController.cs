using AESMovilAPI.DTOs;
using AESMovilAPI.Responses;
using AESMovilAPI.Utilities;
using iText.IO.Image;
using iText.Kernel.Colors;
using iText.Kernel.Font;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Borders;
using iText.Layout.Element;
using iText.Layout.Properties;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.Text;

namespace AESMovilAPI.Controllers
{
    [Route("api/v1/[controller]")]
    public class FileController : BaseController
    {
        private const int _PREVIUS_YEARS = 2;

        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly PdfFont _fontRegular;
        private readonly PdfFont _fontMedium;
        private readonly PdfFont _fontSemiBold;
        private readonly PdfFont _fontBold;

        private readonly string? _baseEBillinApi;
        private readonly PDFBuilder _builder;

        public FileController(IConfiguration config, IHttpClientFactory httpClientFactory, IWebHostEnvironment webHostEnvironment, IMemoryCache cache, PDFBuilder builder) : base(config, httpClientFactory, cache)
        {
            _webHostEnvironment = webHostEnvironment;

            string fontPathRegular = Path.Combine(_webHostEnvironment.ContentRootPath, "Sources", "Fonts", "PublicSans-Regular.otf"); // Path to the custom font file
            string fontPathMedium = Path.Combine(_webHostEnvironment.ContentRootPath, "Sources", "Fonts", "PublicSans-Medium.otf"); // Path to the custom font file
            string fontPathSemiBold = Path.Combine(_webHostEnvironment.ContentRootPath, "Sources", "Fonts", "PublicSans-SemiBold.otf"); // Path to the custom font file
            string fontPathBold = Path.Combine(_webHostEnvironment.ContentRootPath, "Sources", "Fonts", "PublicSans-Bold.otf"); // Path to the custom font file

            _fontRegular = PdfFontFactory.CreateFont(fontPathRegular, PdfFontFactory.EmbeddingStrategy.PREFER_EMBEDDED);
            _fontMedium = PdfFontFactory.CreateFont(fontPathMedium, PdfFontFactory.EmbeddingStrategy.PREFER_EMBEDDED);
            _fontSemiBold = PdfFontFactory.CreateFont(fontPathSemiBold, PdfFontFactory.EmbeddingStrategy.PREFER_EMBEDDED);
            _fontBold = PdfFontFactory.CreateFont(fontPathBold, PdfFontFactory.EmbeddingStrategy.PREFER_EMBEDDED);

            _baseEBillinApi = _config.GetValue<string>("EBillingAPI:Base");
            _builder = builder;
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetFile(string id)
        {
            if (!string.IsNullOrEmpty(id.Trim()))
            {
                id = AESEncryption.AES.GetDecrypt(id, Constants.ENCRYPT_KEY, Constants.SECRECT_KEY_IV);

                FileInfoDto data = JsonConvert.DeserializeObject<FileInfoDto>(id)!;   // !: indica que no sera null en tiempo de ejecucion

                try
                {
                    switch (data.Type)
                    {
                        case "dte":
                            var dataResult = await BuildDteFile(data.DocumentNumber);
                            if (dataResult.DataByte != null)
                            {
                                return File(dataResult.DataByte, "application/json", dataResult.Name);
                            }
                            break;
                        case "invoice":
                            var dataFileResult = await BuildBillFile(data.DocumentNumber);
                            // Agrega los timepos en header personalizados a la respuesta
                            Response.Headers.Add("X-Interface-Time-ms", dataFileResult.InterfaceTime.ToString());
                            Response.Headers.Add("X-Builder-Time-ms", dataFileResult.BuildTime.ToString());

                            if (dataFileResult.DataByte != null)
                            {
                                return File(dataFileResult.DataByte, "application/pdf", dataFileResult.Name);
                            }
                            break;
                        default: break;
                    }
                }
                catch (Exception ex)
                {

                }
            }
            _statusCode = NOT_FOUND_404;

            return GetResponse(new Response<string>());
        }

        [NonAction]
        [HttpGet("Bill/{id}")]
        public IActionResult GetBill(string id)
        {
            return NotFound();
        }

        /// <summary>
        /// Obtener Json DTE certificado por el Ministerio de Hacienda
        /// </summary>
        /// <param name="id">Número de documento</param>
        /// <returns>Archivo Json</returns>
        /// <response code="200">Correcto.</response>
        /// <response code="401">Error por token de autorización.</response>
        /// <response code="404">No se encontró certificación.</response>
        /// <response code="500">Ha ocurrido un error faltal en el servicio.</response>
        /// <response code="502">Incidente en el servicio.</response>
        [NonAction]
        [HttpGet("Dte/{id}")]
        public async Task<IActionResult> GetDte(string id)
        {
            if (!string.IsNullOrEmpty(id))
            {
                var dataResult = await BuildDteFile(id);
                if (dataResult.DataByte != null)
                {
                    return File(dataResult.DataByte, "application/json", dataResult.Name);
                }
            }
            _statusCode = NOT_FOUND_404;

            return GetResponse(new Response<string>());
        }
        /// <summary>
        /// Obtener reporte PDF de historico de facturación
        /// </summary>
        /// <param name="id">NC del cliente</param>
        /// <param name="fromDate">Desde que fecha se consulta el historico en formato <c>yyyyMMdd</c> por ejemplo <c>20200101</c>. Sino se especifica se tomará 2 años atras, a partir de la fecha actual.</param>
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
                fromDate = DateTime.Now.AddYears(-_PREVIUS_YEARS).ToString("yyyyMMdd");
            }

            if (string.IsNullOrEmpty(toDate) || string.IsNullOrEmpty(toDate.Trim(',')))
            {
                toDate = DateTime.Now.ToString("yyyyMMdd");
            }

            if (!string.IsNullOrEmpty(id))
            {
                string endpoint = "BIL_BILLIMAGEPREVIEWES_AZUREAPPSERVICES_TO_SAPCIS;v=1/InvHistSummarySet(Nic='" + id + "',Ab='" + fromDate + "',Bis='" + toDate + "')";
                dynamic? result = await ExecuteGetRequestSAP(endpoint);

                if (result != null)
                {
                    dynamic data = new
                    {
                        values = result.data.DataSet.results,
                        name = result.data.DataSet.results[0].Cliente,
                        address = result.data.DataSet.results[0].DireccionCliente,
                        fee = result.data.DataSet.results[0].TipoTarifa,
                        company = result.data.DataSet.results[0].Sociedad
                    };
                    byte[] fileBytes = BuildBillingHistoryFile(id, fromDate, toDate, data);
                    return File(fileBytes, "application/pdf", id + "hf.pdf");
                }
                else
                {
                    return NotFound();
                }
            }
            else
            {
                return BadRequest();
            }
        }

        /// <summary>
        /// Obtener reporte PDF de historico de alcaldía
        /// </summary>
        /// <param name="id">NC del cliente</param>
        /// <param name="fromDate">Desde que fecha se consulta el historico en formato <c>yyyyMMdd</c> por ejemplo <c>20200101</c>. Sino se especifica se tomará 2 años atras, a partir de la fecha actual.</param>
        /// <param name="toDate">Hasta que fecha se consulta el historico en formato <c>yyyyMMdd</c> por ejemplo <c>20240606</c>. Sino se especifica se tomará la fecha actual.</param>
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
                fromDate = DateTime.Now.AddYears(-_PREVIUS_YEARS).ToString("yyyyMMdd");
            }

            if (string.IsNullOrEmpty(toDate) || string.IsNullOrEmpty(toDate.Trim(',')))
            {
                toDate = DateTime.Now.ToString("yyyyMMdd");
            }

            if (!string.IsNullOrEmpty(id))
            {
                string endpoint = "ACC_GETHISTORICOCARGOSALCALDIA_AZUREAPPSSERVICES_TO_SAPCIS;v=1/GetHistoricoAlcaldiaSet(Nic='" + id + "',Fechainicio='" + fromDate + "',Fechafin='" + toDate + "')";
                dynamic? result = await ExecuteGetRequestSAP(endpoint);
                if (result != null)
                {
                    dynamic data = new
                    {
                        values = result.data.DataSet.results,
                        name = result.data.DataSet.results[0].Cliente,
                        address = result.data.DataSet.results[0].Domicilio,
                        fee = result.data.DataSet.results[0].Tarifa,
                        company = result.data.DataSet.results[0].Sociedad
                    };
                    byte[] fileBytes = BuildMayoralHistoryFile(id, fromDate, toDate, data);
                    return File(fileBytes, "application/pdf", id + "ha.pdf");
                }
                else
                {
                    return NotFound();
                }
            }
            else
            {
                return BadRequest();
            }
        }

        #region "MayoralHistory"
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

                            // Title
                            Paragraph header = new Paragraph("HISTORICO DE CARGOS DE ALCALDÍA")
                                .SetFont(_fontBold)
                                .SetTextAlignment(TextAlignment.CENTER)
                                .SetFontSize(10);
                            // New line
                            Paragraph newline = new Paragraph(new Text("\n"));
                            Paragraph dateTitle = new Paragraph("DESDE " + Helper.ParseStrDateMonth(fromDate).ToUpper() + " HASTA " + Helper.ParseStrDateMonth(toDate).ToUpper())
                                .SetFont(_fontSemiBold)
                                .SetFontSize(7)
                                .SetTextAlignment(TextAlignment.CENTER);

                            document.Add(header);
                            document.Add(dateTitle);
                            document.Add(newline);

                            // Add an image (logo) to the document
                            ImageData imageData = ImageDataFactory.Create(Path.Combine(_webHostEnvironment.ContentRootPath, "Sources", "Images", Helper.GetCompanyName(data.company) + "-logo.png"));
                            Image img = new Image(imageData);
                            img.ScaleToFit(100, 100);
                            img.SetFixedPosition(456, 784);

                            document.Add(img);

                            // Header
                            Color headerColor = new DeviceRgb(229, 229, 229);
                            Color lineColor = new DeviceRgb(245, 245, 245);
                            Color lightColor = new DeviceRgb(245, 245, 245);
                            Table tableHeader = new Table(12, true);
                            Cell cellH11 = new Cell(1, 2)
                                .SetBorder(Border.NO_BORDER)
                                .SetTextAlignment(TextAlignment.LEFT)
                                .Add(new Paragraph("NOMBRE:").SetFont(_fontSemiBold).SetFontSize(7));
                            Cell cellH12 = new Cell(1, 7)
                                .SetBorder(Border.NO_BORDER)
                                .SetTextAlignment(TextAlignment.LEFT)
                                .Add(new Paragraph(data.name).SetFont(_fontMedium).SetFontSize(7));
                            //Cell cellH13 = new Cell(1, 1)
                            //    .SetBorder(Border.NO_BORDER)
                            //    .SetTextAlignment(TextAlignment.LEFT)
                            //    .Add(new Paragraph("NC:").SetFont(_fontSemiBold).SetFontSize(7));
                            //Cell cellH14 = new Cell(1, 2)
                            //    .SetBorder(Border.NO_BORDER)
                            //    .SetTextAlignment(TextAlignment.LEFT)
                            //    .Add(new Paragraph(nc).SetFont(_fontMedium).SetFontSize(7));
                            Cell cellH15 = new Cell(1, 1)
                                .SetBorder(Border.NO_BORDER)
                                .SetBackgroundColor(lightColor)
                                .SetTextAlignment(TextAlignment.LEFT)
                                .Add(new Paragraph("FECHA:").SetFont(_fontSemiBold).SetFontSize(7));
                            Cell cellH16 = new Cell(1, 2)
                                .SetBorder(Border.NO_BORDER)
                                .SetBackgroundColor(lightColor)
                                .SetTextAlignment(TextAlignment.LEFT)
                                .Add(new Paragraph(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")).SetFont(_fontMedium).SetFontSize(7));

                            Cell cellH21 = new Cell(1, 2)
                                .SetBorder(Border.NO_BORDER)
                                .SetTextAlignment(TextAlignment.LEFT)
                                .Add(new Paragraph("DIRECCIÓN:").SetFont(_fontSemiBold).SetFontSize(7));
                            Cell cellH22 = new Cell(1, 7)
                                .SetBorder(Border.NO_BORDER)
                                .SetTextAlignment(TextAlignment.LEFT)
                                .Add(new Paragraph(data.address).SetFont(_fontMedium).SetFontSize(7));
                            //Cell cellH23 = new Cell(1, 2)
                            //    .SetBorder(Border.NO_BORDER)
                            //    .SetTextAlignment(TextAlignment.LEFT)
                            //    .Add(new Paragraph("SERVICIOS:").SetFont(_fontSemiBold).SetFontSize(7));
                            //Cell cellH24 = new Cell(1, 1)
                            //    .SetBorder(Border.NO_BORDER)
                            //    .SetTextAlignment(TextAlignment.LEFT)
                            //    .Add(new Paragraph("Todos.").SetFont(_fontMedium).SetFontSize(7));
                            Cell cellH25 = new Cell(1, 1)
                                .SetBorder(Border.NO_BORDER)
                                .SetTextAlignment(TextAlignment.LEFT)
                                .Add(new Paragraph("TARIFA:").SetFont(_fontSemiBold).SetFontSize(7));
                            Cell cellH26 = new Cell(1, 2)
                                .SetBorder(Border.NO_BORDER)
                                .SetTextAlignment(TextAlignment.LEFT)
                                .Add(new Paragraph(data.fee).SetFont(_fontMedium).SetFontSize(7));

                            Cell cellH31 = new Cell(1, 2)
                                .SetBorder(Border.NO_BORDER)
                                .SetTextAlignment(TextAlignment.LEFT)
                                .Add(new Paragraph("CUENTA CONTRATO:").SetFont(_fontSemiBold).SetFontSize(7));
                            Cell cellH32 = new Cell(1, 7)
                                .SetBorder(Border.NO_BORDER)
                                .SetTextAlignment(TextAlignment.LEFT)
                                .Add(new Paragraph(nc).SetFont(_fontMedium).SetFontSize(7));
                            Cell cellH35 = new Cell(1, 1)
                                .SetBorder(Border.NO_BORDER)
                                .SetTextAlignment(TextAlignment.LEFT)
                                .Add(new Paragraph("SERVICIO:").SetFont(_fontSemiBold).SetFontSize(7));
                            Cell cellH36 = new Cell(1, 2)
                                .SetBorder(Border.NO_BORDER)
                                .SetTextAlignment(TextAlignment.LEFT)
                                .Add(new Paragraph("Todos").SetFont(_fontMedium).SetFontSize(7));

                            tableHeader.AddCell(cellH11);
                            tableHeader.AddCell(cellH12);
                            //tableHeader.AddCell(cellH13);
                            //tableHeader.AddCell(cellH14);
                            tableHeader.AddCell(cellH15);
                            tableHeader.AddCell(cellH16);
                            tableHeader.AddCell(cellH21);
                            tableHeader.AddCell(cellH22);
                            //tableHeader.AddCell(cellH23);
                            //tableHeader.AddCell(cellH24);
                            tableHeader.AddCell(cellH25);
                            tableHeader.AddCell(cellH26);
                            tableHeader.AddCell(cellH31);
                            tableHeader.AddCell(cellH32);
                            tableHeader.AddCell(cellH35);
                            tableHeader.AddCell(cellH36);

                            document.Add(tableHeader);
                            document.Add(newline);
                            // Line separator
                            //LineSeparator ls = new LineSeparator(new SolidLine()).SetBackgroundColor(lineColor);
                            //document.Add(ls);

                            // Table
                            Table table = new Table(7, true);
                            Cell cell11 = new Cell(1, 1)
                                .SetBorder(Border.NO_BORDER)
                                //.SetBorderBottom(new SolidBorder(lineColor, 1))
                                .SetBackgroundColor(headerColor)
                                .SetTextAlignment(TextAlignment.CENTER)
                                .Add(new Paragraph("CARGO")
                                .SetFont(_fontBold)
                                .SetFontSize(7));
                            Cell cell12 = new Cell(1, 1)
                                .SetBorder(Border.NO_BORDER)
                                //.SetBorderBottom(new SolidBorder(lineColor, 1))
                                .SetBackgroundColor(headerColor)
                                .SetTextAlignment(TextAlignment.CENTER)
                                .Add(new Paragraph("FECHA DE FACTURACIÓN")
                                .SetFont(_fontBold)
                                .SetFontSize(7));
                            Cell cell13 = new Cell(1, 1)
                                .SetBorder(Border.NO_BORDER)
                               //.SetBorderBottom(new SolidBorder(lineColor, 1))
                               .SetBackgroundColor(headerColor)
                               .SetTextAlignment(TextAlignment.CENTER)
                               .Add(new Paragraph("MONTO FACTURADO")
                               .SetFont(_fontBold)
                               .SetFontSize(7));
                            //Cell cell14 = new Cell(1, 1)
                            //    .SetBorder(Border.NO_BORDER)
                            //    .SetBorderBottom(new SolidBorder(ColorConstants.GRAY, 1))
                            //   //.SetBackgroundColor(ColorConstants.LIGHT_GRAY)
                            //   .SetTextAlignment(TextAlignment.CENTER)
                            //   .Add(new Paragraph("FECHA DE VENCIMIENTO").SetBold().SetFontSize(8));
                            Cell cell15 = new Cell(1, 1)
                                .SetBorder(Border.NO_BORDER)
                               //.SetBorderBottom(new SolidBorder(lineColor, 1))
                               .SetBackgroundColor(headerColor)
                               .SetTextAlignment(TextAlignment.CENTER)
                               .Add(new Paragraph("FECHA DE PAGO")
                               .SetFont(_fontBold)
                               .SetFontSize(7));
                            Cell cell16 = new Cell(1, 1)
                                .SetBorder(Border.NO_BORDER)
                               //.SetBorderBottom(new SolidBorder(lineColor, 1))
                               .SetBackgroundColor(headerColor)
                               .SetTextAlignment(TextAlignment.CENTER)
                               .Add(new Paragraph("ESTADO")
                               .SetFont(_fontBold)
                               .SetFontSize(7));
                            Cell cell17 = new Cell(1, 1)
                                .SetBorder(Border.NO_BORDER)
                               //.SetBorderBottom(new SolidBorder(lineColor, 1))
                               .SetBackgroundColor(headerColor)
                               .SetTextAlignment(TextAlignment.LEFT)
                               .Add(new Paragraph("ALCALDÍA")
                               .SetFont(_fontBold)
                               .SetFontSize(7));
                            Cell cell18 = new Cell(1, 1)
                                .SetBorder(Border.NO_BORDER)
                               //.SetBorderBottom(new SolidBorder(lineColor, 1))
                               .SetBackgroundColor(headerColor)
                               .SetTextAlignment(TextAlignment.LEFT)
                               .Add(new Paragraph("CUENTA")
                               .SetFont(_fontBold)
                               .SetFontSize(7));


                            table.AddCell(cell11);
                            table.AddCell(cell12);
                            table.AddCell(cell13);
                            //table.AddCell(cell14);
                            table.AddCell(cell15);
                            table.AddCell(cell16);
                            table.AddCell(cell17);
                            table.AddCell(cell18);

                            var sortedList = ((List<dynamic>)data.values).OrderByDescending(obj =>
                            {
                                DateTime dateValue;
                                return DateTime.TryParse(obj.FechaFacturado, out dateValue) ? dateValue : DateTime.MinValue;
                            })
                            .ToList();

                            foreach (var item in sortedList)
                            {
                                Cell cell1 = new Cell(1, 1)
                                    .SetBorder(Border.NO_BORDER)
                                    .SetTextAlignment(TextAlignment.LEFT)
                                    .Add(new Paragraph(item.CargoVario).SetFont(_fontRegular).SetFontSize(7));
                                Cell cell2 = new Cell(1, 1)
                                    .SetBorder(Border.NO_BORDER)
                                    .SetTextAlignment(TextAlignment.CENTER)
                                    .Add(new Paragraph(Helper.ParseStrDate(item.FechaFacturado)).SetFont(_fontRegular).SetFontSize(7));
                                Cell cell3 = new Cell(1, 1)
                                    .SetBorder(Border.NO_BORDER)
                                    .SetTextAlignment(TextAlignment.RIGHT)
                                    .Add(new Paragraph(Helper.MoneyValue(item.Monto.Trim())).SetFont(_fontRegular).SetFontSize(7));
                                //Cell cell4 = new Cell(1, 1)
                                //    .SetBorder(Border.NO_BORDER)
                                //    .SetTextAlignment(TextAlignment.LEFT)
                                //    .Add(new Paragraph(Helper.ParseStrDate2(item.FechaCobro)).SetFontSize(8));
                                Cell cell5 = new Cell(1, 1)
                                    .SetBorder(Border.NO_BORDER)
                                    .SetTextAlignment(TextAlignment.CENTER)
                                    .Add(new Paragraph(Helper.ParseStrDate(item.FechaCobro)).SetFont(_fontRegular).SetFontSize(7));
                                Cell cell6 = new Cell(1, 1)
                                    .SetBorder(Border.NO_BORDER)
                                    .SetTextAlignment(TextAlignment.CENTER)
                                    .Add(new Paragraph(item.Estado).SetFont(_fontRegular).SetFontSize(7));
                                Cell cell7 = new Cell(1, 1)
                                .SetBorder(Border.NO_BORDER)
                                    .SetTextAlignment(TextAlignment.LEFT)
                                    .Add(new Paragraph(item.Alcaldia).SetFont(_fontRegular).SetFontSize(7));
                                Cell cell8 = new Cell(1, 1)
                                    .SetBorder(Border.NO_BORDER)
                                    .SetTextAlignment(TextAlignment.LEFT)
                                    .Add(new Paragraph(item.Cuenta).SetFont(_fontRegular).SetFontSize(7));

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

        #region "BillingHistory"
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

                            // Title
                            Paragraph header = new Paragraph("HISTORICO DE FACTURACIÓN")
                                .SetFont(_fontBold)
                                .SetTextAlignment(TextAlignment.CENTER)
                                .SetFontSize(10);
                            // New line
                            Paragraph newline = new Paragraph(new Text("\n"));
                            Paragraph dateTitle = new Paragraph("DESDE " + Helper.ParseStrDateMonth(fromDate).ToUpper() + " HASTA " + Helper.ParseStrDateMonth(toDate).ToUpper())
                                .SetFont(_fontSemiBold)
                                .SetFontSize(7)
                                .SetTextAlignment(TextAlignment.CENTER);

                            document.Add(header);
                            document.Add(dateTitle);
                            document.Add(newline);

                            // Add an image (logo) to the document
                            ImageData imageData = ImageDataFactory.Create(Path.Combine(_webHostEnvironment.ContentRootPath, "Sources", "Images", Helper.GetCompanyName(data.company) + "-logo.png"));
                            Image img = new Image(imageData);
                            img.ScaleToFit(100, 100);
                            img.SetFixedPosition(456, 784);

                            document.Add(img);

                            // Header
                            Color headerColor = new DeviceRgb(229, 229, 229);
                            Color lineColor = new DeviceRgb(245, 245, 245);
                            Color lightColor = new DeviceRgb(245, 245, 245);
                            Table tableHeader = new Table(12, true);
                            Cell cellH11 = new Cell(1, 2)
                                .SetBorder(Border.NO_BORDER)
                                .SetTextAlignment(TextAlignment.LEFT)
                                .Add(new Paragraph("NOMBRE:").SetFont(_fontSemiBold).SetFontSize(7));
                            Cell cellH12 = new Cell(1, 7)
                                .SetBorder(Border.NO_BORDER)
                                .SetTextAlignment(TextAlignment.LEFT)
                                .Add(new Paragraph(data.name).SetFont(_fontMedium).SetFontSize(7));
                            //Cell cellH13 = new Cell(1, 1)
                            //    .SetBorder(Border.NO_BORDER)
                            //    .SetTextAlignment(TextAlignment.LEFT)
                            //    .Add(new Paragraph("NC:").SetFont(_fontSemiBold).SetFontSize(7));
                            //Cell cellH14 = new Cell(1, 2)
                            //    .SetBorder(Border.NO_BORDER)
                            //    .SetTextAlignment(TextAlignment.LEFT)
                            //    .Add(new Paragraph(nc).SetFont(_fontMedium).SetFontSize(7));
                            Cell cellH15 = new Cell(1, 1)
                                .SetBorder(Border.NO_BORDER)
                                .SetBackgroundColor(lightColor)
                                .SetTextAlignment(TextAlignment.LEFT)
                                .Add(new Paragraph("FECHA:").SetFont(_fontSemiBold).SetFontSize(7));
                            Cell cellH16 = new Cell(1, 2)
                                .SetBorder(Border.NO_BORDER)
                                .SetBackgroundColor(lightColor)
                                .SetTextAlignment(TextAlignment.LEFT)
                                .Add(new Paragraph(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")).SetFont(_fontMedium).SetFontSize(7));

                            Cell cellH21 = new Cell(1, 2)
                                .SetBorder(Border.NO_BORDER)
                                .SetTextAlignment(TextAlignment.LEFT)
                                .Add(new Paragraph("DIRECCIÓN:").SetFont(_fontSemiBold).SetFontSize(7));
                            Cell cellH22 = new Cell(1, 7)
                                .SetBorder(Border.NO_BORDER)
                                .SetTextAlignment(TextAlignment.LEFT)
                                .Add(new Paragraph(data.address).SetFont(_fontMedium).SetFontSize(7));
                            //Cell cellH23 = new Cell(1, 2)
                            //    .SetBorder(Border.NO_BORDER)
                            //    .SetTextAlignment(TextAlignment.LEFT)
                            //    .Add(new Paragraph("SERVICIOS:").SetFont(_fontSemiBold).SetFontSize(7));
                            //Cell cellH24 = new Cell(1, 1)
                            //    .SetBorder(Border.NO_BORDER)
                            //    .SetTextAlignment(TextAlignment.LEFT)
                            //    .Add(new Paragraph("Todos.").SetFont(_fontMedium).SetFontSize(7));
                            Cell cellH25 = new Cell(1, 1)
                                .SetBorder(Border.NO_BORDER)
                                .SetTextAlignment(TextAlignment.LEFT)
                                .Add(new Paragraph("TARIFA:").SetFont(_fontSemiBold).SetFontSize(7));
                            Cell cellH26 = new Cell(1, 2)
                                .SetBorder(Border.NO_BORDER)
                                .SetTextAlignment(TextAlignment.LEFT)
                                .Add(new Paragraph(data.fee).SetFont(_fontMedium).SetFontSize(7));

                            Cell cellH31 = new Cell(1, 2)
                                .SetBorder(Border.NO_BORDER)
                                .SetTextAlignment(TextAlignment.LEFT)
                                .Add(new Paragraph("CUENTA CONTRATO:").SetFont(_fontSemiBold).SetFontSize(7));
                            Cell cellH32 = new Cell(1, 7)
                                .SetBorder(Border.NO_BORDER)
                                .SetTextAlignment(TextAlignment.LEFT)
                                .Add(new Paragraph(nc).SetFont(_fontMedium).SetFontSize(7));
                            Cell cellH35 = new Cell(1, 1)
                                .SetBorder(Border.NO_BORDER)
                                .SetTextAlignment(TextAlignment.LEFT)
                                .Add(new Paragraph("SERVICIO:").SetFont(_fontSemiBold).SetFontSize(7));
                            Cell cellH36 = new Cell(1, 2)
                                .SetBorder(Border.NO_BORDER)
                                .SetTextAlignment(TextAlignment.LEFT)
                                .Add(new Paragraph("Todos").SetFont(_fontMedium).SetFontSize(7));

                            tableHeader.AddCell(cellH11);
                            tableHeader.AddCell(cellH12);
                            //tableHeader.AddCell(cellH13);
                            //tableHeader.AddCell(cellH14);
                            tableHeader.AddCell(cellH15);
                            tableHeader.AddCell(cellH16);
                            tableHeader.AddCell(cellH21);
                            tableHeader.AddCell(cellH22);
                            //tableHeader.AddCell(cellH23);
                            //tableHeader.AddCell(cellH24);
                            tableHeader.AddCell(cellH25);
                            tableHeader.AddCell(cellH26);
                            tableHeader.AddCell(cellH31);
                            tableHeader.AddCell(cellH32);
                            tableHeader.AddCell(cellH35);
                            tableHeader.AddCell(cellH36);

                            document.Add(tableHeader);
                            document.Add(newline);
                            // Line separator
                            //LineSeparator ls = new LineSeparator(new SolidLine()).SetBackgroundColor(lineColor);
                            //document.Add(ls);

                            // Table
                            Table table = new Table(7, false);
                            Cell cell11 = new Cell(1, 1)
                                .SetBorder(Border.NO_BORDER)
                                //.SetBorderBottom(new SolidBorder(lineColor, 1))
                                .SetBackgroundColor(headerColor)
                                .SetTextAlignment(TextAlignment.CENTER)
                                .Add(new Paragraph("NÚMERO DE FACTURA")
                                .SetFont(_fontBold)
                                .SetFontSize(7));
                            Cell cell12 = new Cell(1, 1)
                                .SetBorder(Border.NO_BORDER)
                                //.SetBorderBottom(new SolidBorder(lineColor, 1))
                                .SetBackgroundColor(headerColor)
                                .SetTextAlignment(TextAlignment.CENTER)
                                .Add(new Paragraph("FECHA DE FACTURACIÓN")
                                .SetFont(_fontBold)
                                .SetFontSize(7));
                            Cell cell13 = new Cell(1, 1)
                                .SetBorder(Border.NO_BORDER)
                               //.SetBorderBottom(new SolidBorder(lineColor, 1))
                               .SetBackgroundColor(headerColor)
                               .SetTextAlignment(TextAlignment.CENTER)
                               .Add(new Paragraph("MONTO FACTURADO")
                               .SetFont(_fontBold)
                               .SetFontSize(7));
                            //Cell cell14 = new Cell(1, 1)
                            //    .SetBorder(Border.NO_BORDER)
                            //    .SetBorderBottom(new SolidBorder(ColorConstants.GRAY, 1))
                            //   //.SetBackgroundColor(ColorConstants.LIGHT_GRAY)
                            //   .SetTextAlignment(TextAlignment.CENTER)
                            //   .Add(new Paragraph("FECHA DE VENCIMIENTO").SetBold().SetFontSize(8));
                            Cell cell15 = new Cell(1, 1)
                                .SetBorder(Border.NO_BORDER)
                               //.SetBorderBottom(new SolidBorder(lineColor, 1))
                               .SetBackgroundColor(headerColor)
                               .SetTextAlignment(TextAlignment.CENTER)
                               .Add(new Paragraph("MONTO CANCELADO")
                               .SetFont(_fontBold)
                               .SetFontSize(7));
                            Cell cell16 = new Cell(1, 1)
                                .SetBorder(Border.NO_BORDER)
                               //.SetBorderBottom(new SolidBorder(lineColor, 1))
                               .SetBackgroundColor(headerColor)
                               .SetTextAlignment(TextAlignment.CENTER)
                               .Add(new Paragraph("FECHA DE PAGO")
                               .SetFont(_fontBold)
                               .SetFontSize(7));
                            Cell cell17 = new Cell(1, 1)
                                .SetBorder(Border.NO_BORDER)
                               //.SetBorderBottom(new SolidBorder(lineColor, 1))
                               .SetBackgroundColor(headerColor)
                               .SetTextAlignment(TextAlignment.LEFT)
                               .Add(new Paragraph("ESTADO")
                               .SetFont(_fontBold)
                               .SetFontSize(7));
                            Cell cell18 = new Cell(1, 1)
                                .SetBorder(Border.NO_BORDER)
                               //.SetBorderBottom(new SolidBorder(lineColor, 1))
                               .SetBackgroundColor(headerColor)
                               .SetTextAlignment(TextAlignment.LEFT)
                               .Add(new Paragraph("LUGAR DE PAGO")
                               .SetFont(_fontBold)
                               .SetFontSize(7));


                            table.AddCell(cell11);
                            table.AddCell(cell12);
                            table.AddCell(cell13);
                            //table.AddCell(cell14);
                            table.AddCell(cell15);
                            table.AddCell(cell16);
                            table.AddCell(cell17);
                            table.AddCell(cell18);

                            var sortedList = ((List<dynamic>)data.values).OrderByDescending(obj =>
                            {
                                DateTime dateValue;
                                return DateTime.TryParse(obj.FechaFacturacion, out dateValue) ? dateValue : DateTime.MinValue;
                            })
                            .ToList();

                            foreach (var item in sortedList)
                            {
                                Cell cell1 = new Cell(1, 1)
                                    .SetBorder(Border.NO_BORDER)
                                    .SetTextAlignment(TextAlignment.CENTER)
                                    .Add(new Paragraph(item.NumRecibo).SetFont(_fontRegular).SetFontSize(7));
                                Cell cell2 = new Cell(1, 1)
                                    .SetBorder(Border.NO_BORDER)
                                    .SetTextAlignment(TextAlignment.CENTER)
                                    .Add(new Paragraph(item.FechaFacturacion).SetFont(_fontRegular).SetFontSize(7));
                                Cell cell3 = new Cell(1, 1)
                                    .SetBorder(Border.NO_BORDER)
                                    .SetTextAlignment(TextAlignment.RIGHT)
                                    .Add(new Paragraph(Helper.MoneyValue(item.ImpFact.Trim())).SetFont(_fontRegular).SetFontSize(7));
                                //Cell cell4 = new Cell(1, 1)
                                //    .SetBorder(Border.NO_BORDER)
                                //    .SetTextAlignment(TextAlignment.LEFT)
                                //    .Add(new Paragraph(Helper.ParseStrDate2(item.FechaCobro)).SetFontSize(8));
                                Cell cell5 = new Cell(1, 1)
                                    .SetBorder(Border.NO_BORDER)
                                    .SetTextAlignment(TextAlignment.RIGHT)
                                    .Add(new Paragraph(Helper.MoneyValue(item.ImpFactCanc.Trim())).SetFont(_fontRegular).SetFontSize(7));
                                Cell cell6 = new Cell(1, 1)
                                    .SetBorder(Border.NO_BORDER)
                                    .SetTextAlignment(TextAlignment.CENTER)
                                    .Add(new Paragraph(item.FechaPago).SetFont(_fontRegular).SetFontSize(7));
                                Cell cell7 = new Cell(1, 1)
                                .SetBorder(Border.NO_BORDER)
                                    .SetTextAlignment(TextAlignment.LEFT)
                                    .Add(new Paragraph(item.Status).SetFont(_fontRegular).SetFontSize(7));
                                Cell cell8 = new Cell(1, 1)
                                    .SetBorder(Border.NO_BORDER)
                                    .SetTextAlignment(TextAlignment.LEFT)
                                    .Add(new Paragraph(item.LugarPago).SetFont(_fontRegular).SetFontSize(7));

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
        private async Task<FileDataDto> BuildDteFile(string id)
        {
            byte[]? result = null;
            string name = "";

            id = id.PadLeft(14, '0');

            var postData = new
            {
                dte = id
            };


            string url = _baseEBillinApi + "/api/dte/find";
            string? token = GetToken(Constants.EBILLAPI_BEARER);

            if (token == null)
            {
                token = await GetEbillingAPIToken();
            }

            dynamic? responseObject = await ExecutePostRequest(postData, url, true, token);

            // Retry request with token
            try
            {
                if (responseObject == UNAUTHORIZED_401)
                {
                    token = await GetEbillingAPIToken();
                    responseObject = await ExecutePostRequest(postData, url, true, token);
                }
            }
            catch (Exception ex)
            {

            }

            if (responseObject != null && responseObject.code == OK_200)
            {
                string json = JsonConvert.SerializeObject(responseObject.data.json_dte);
                // Convertir el string en bytes
                result = Encoding.UTF8.GetBytes(json);
                name = responseObject.data.vl_controlnumber + ".json";
            }

            return new FileDataDto { DataByte = result, Name = name };
        }
        private async Task<FileDataDto> BuildBillFile(string id)
        {
            string rootPath = _webHostEnvironment.ContentRootPath;
            string baseUrl = _config.GetValue<string>(Constants.CONF_SAP_BASE);
            string mandante = _config.GetValue<string>(Constants.CONF_SAP_ENVIRONMENT);
            string endpoint = baseUrl + "/gw/odata/SAP/CIS" + mandante + $"_ACC_GETINVOICEFORMJSON_AZUREAPPSSERVICES_TO_SAPCIS;v=1/GetInvoiceToJsonSet('{id}')";

            // Inicia el temporizador
            var stopwatch = Stopwatch.StartNew();
            dynamic? response = await ExecuteGetRequest(endpoint, true, null, false, false);

            stopwatch.Stop();

            var elapsedMillisecondsRequest = stopwatch.ElapsedMilliseconds;
            string xmlString = Helper.CleanXml(response, "http://www.w3.org/2005/Atom");
            var entry = Helper.DeserializeXml<Entry>(xmlString)!;
            JObject jsonObject = JObject.Parse(entry.Content.Properties.Json);

            stopwatch.Restart();
            byte[]? result = _builder.DoFillFormByte(jsonObject["Cabecera"].ToString(), jsonObject["Items"].ToString());

            stopwatch.Stop();
            var elapsedMillisecondsBuilder = stopwatch.ElapsedMilliseconds;

            return new FileDataDto { DataByte = result, Name = id + ".pdf", InterfaceTime = elapsedMillisecondsRequest, BuildTime = elapsedMillisecondsBuilder };
        }
        private async Task<string> GetEbillingAPIToken()
        {
            string url = _baseEBillinApi + "/api/userapi";
            string token = string.Empty;

            var postData = new
            {
                user = _config.GetValue<string>("EBillingAPI:Usr"),
                pass = _config.GetValue<string>("EBillingAPI:Pwd")
            };
            dynamic? responseObject = await ExecutePostRequest(postData, url, false);

            if (responseObject != null && responseObject.code == OK_200)
            {
                token = responseObject.data.token;
                SaveToken(Constants.EBILLAPI_BEARER, token);
            }

            return token;
        }
    }
}