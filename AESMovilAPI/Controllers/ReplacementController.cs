﻿using AESMovilAPI.DTOs;
using AESMovilAPI.Responses;
using AESMovilAPI.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;

namespace AESMovilAPI.Controllers
{
    [Route("api/v1/[controller]")]
    public class ReplacementController : BaseController
    {
        public ReplacementController(IConfiguration config, HttpClient httpClient, IMemoryCache cache) : base(config, httpClient, cache)
        {
        }

        /// <summary>
        /// Obtener links para descargar PDF de factura y DTE certificado por el Ministerio de Hacienda
        /// </summary>
        /// <param name="id">Cuenta contrato</param>
        /// <returns>Links para descargar PDF y JSON</returns>
        /// <response code="200">Correcto.</response>
        /// <response code="401">Error por token de autorización.</response>
        /// <response code="404">No se encontro información.</response>
        /// <response code="500">Ha ocurrido un error faltal en el servicio.</response>
        /// <response code="502">Incidente en el servicio.</response>
        [HttpGet("{id}")]
        public async Task<IActionResult> Index(string id)
        {
            Response<ReplacementResponse> response = new Response<ReplacementResponse>();

            if (!string.IsNullOrEmpty(id))
            {
                string fromDate = DateTime.Now.AddMonths(-6).ToString("yyyyMMdd");
                string toDate = DateTime.Now.ToString("yyyyMMdd");
                string endpoint = "BIL_BILLIMAGEPREVIEWES_AZUREAPPSERVICES_TO_SAPCIS;v=1/InvHistSummarySet(Nic='" + id + "',Ab='" + fromDate + "',Bis='" + toDate + "')";
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

                    }

                    if (data != null)
                    {
                        var sortedList = ((List<dynamic>)data.values).OrderByDescending(obj =>
                        {
                            DateTime dateValue;
                            return DateTime.TryParse(obj.FechaFacturacion, out dateValue) ? dateValue : DateTime.MinValue;
                        }).ToList();

                        string url = _config.GetValue<string>("Replacement:Base") + "/api/v1/file/";
                        string documentNumber = sortedList.ElementAt(0).NumRecibo;
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
                else
                {
                    _statusCode = NOT_FOUND_404;
                }
            }

            return GetResponse(response);
        }
    }
}
