using AESMovilAPI.DTOs;
using AESMovilAPI.Examples;
using AESMovilAPI.Models;
using AESMovilAPI.Responses;
using AESMovilAPI.Services;
using AESMovilAPI.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Filters;
using System.Numerics;

namespace AESMovilAPI.Controllers
{
    public class SearchController : BaseController<SearchController>
    {
        private readonly SAPSGCDbContext _db;
        public SearchController(IConfiguration config, LoggerService<SearchController> logger, SAPSGCDbContext db) : base(config, logger)
        {
            _db = db;
        }

        /// <summary>
        /// Consulta información de Cuenta contrato o NIC.
        /// </summary>
        /// <remarks>
        /// **No recomendado**: Este endpoint es legacy y será deshabilitado por seguridad.
        /// </remarks>
        /// <param name="query">Número de Cuenta, NIC o NPE, representado por el objeto <see cref="Query">Query</see></param>
        /// <returns>información de contrato o NIC</returns>
        /// <response code="200">Correcto</response>
        /// <response code="500">Ha ocurrido un error faltal en el servicio.</response>
        // GET: api/SAPSGC
        [SwaggerRequestExample(typeof(Query), typeof(SGCSAPExample))]
        [AllowAnonymous]
        [HttpPost]
        [Route("api/v1/Search")]
        [Route("api/v1/SAPSGC")]
        public async Task<IActionResult> GetById(Query query)
        {

            dynamic? response = null;
            BigInteger number;
            string id = query.Cuenta;

            id = Helper.RemoveWhitespaces(id);

            if (id != null && id.Length == 6 && BigInteger.TryParse(id, out number) ||
                id != null && id.Length == 7 && BigInteger.TryParse(id, out number) ||
                id != null && id.Length == 12 && BigInteger.TryParse(id, out number) ||
                id != null && id.Length == 24 && BigInteger.TryParse(id, out number))
            {
                _statusCode = NOT_FOUND_404;

                var data = await GetData(id);

                if (data != null)
                {
                    _statusCode = OK_200;
                    response = new { Data = data, ErrorCode = "0", ErrorMsg = "Servicio ejecutado con exito." };
                }
                else
                {
                    response = new { Data = data, ErrorCode = "4", ErrorMsg = "No hay datos." };
                }
            }

            return Ok(response);
        }

        /// <summary>
        /// Consulta información de Contrato, Cuenta contrato, NIC o NPE.
        /// </summary>
        /// <param name="id">Contrato, Cuenta Contrato, NIC o NPE</param>
        /// <remarks>
        /// **Recomendado usar.**
        /// </remarks>
        /// <returns>Información del cliente</returns>
        /// <response code="200">Solicitud completada con éxito.</response>
        /// <response code="400">Consulta con datos incorrectos.</response>
        /// <response code="401">Error por token de autorización.</response>
        /// <response code="404">No se encontraron datos de Cuenta Contrato o NIC.</response>
        /// <response code="500">Error inesperado en el servicio. Intente nuevamente en unos minutos.</response>
        /// <response code="502">Servicio dependiente no respondió correctamente.</response>
        /// <response code="503">Servicio no disponible en este momento.</response>
        // GET: api/Search
        [HttpGet("api/v1/Search/{id}")]
        public async Task<IActionResult> Get(string id)
        {
            Response<List<SAPSGCResponse>> response = new Response<List<SAPSGCResponse>>();
            BigInteger number;

            id = Helper.RemoveWhitespaces(id);

            if (id != null && id.Length == 6 && BigInteger.TryParse(id, out number) ||
                id != null && id.Length == 7 && BigInteger.TryParse(id, out number) ||
                id != null && id.Length == 10 && BigInteger.TryParse(id, out number) ||
                id != null && id.Length == 12 && BigInteger.TryParse(id, out number) ||
                id != null && id.Length == 24 && BigInteger.TryParse(id, out number))
            {
                var data = await GetData(id);

                if (data != null)
                {
                    _statusCode = OK_200;
                    response.Success = true;
                    response.Data = data;
                }
                else
                {
                    _statusCode = NOT_FOUND_404;
                }
            }
            else
            {
                _statusCode = BAD_REQUEST_400;
            }

            return GetResponse(response);
        }

        private async Task<List<SAPSGCResponse>?> GetData(string id)
        {
            List<SapData> value = new List<SapData>();
            List<SAPSGCResponse>? dataList = null;
            bool fromBD = true;

            if (fromBD)
            {
                try
                {
                    if (id.Length == 24)
                    {
                        // NPE
                        id = id.Substring(12, 7);
                    }

                    if (id.Length == 6 || id.Length == 7)
                    {
                        // NIC
                        value = await _db.SAPData.Where(a => a.Nic == int.Parse(id)).ToListAsync();
                    }
                    else if (id.Length == 10)
                    {
                        // Contrato
                        value = await _db.SAPData.Where(a => a.Vertrag == id).ToListAsync();
                    }
                    else
                    {
                        // NC
                        value = await _db.SAPData.Where(a => a.Vkont == id).ToListAsync();
                    }

                    if (value != null && value.Count > 0)
                    {
                        _statusCode = OK_200;

                        dataList = new List<SAPSGCResponse>();

                        foreach (var a in value)
                        {
                            dataList.Add(new SAPSGCResponse
                            {
                                Nic = a.Nic,
                                NisRad = a.NisRad,
                                SocioComercial = a.Partner,
                                Nombre = a.NameFirst,
                                Apellido = a.NameLast,
                                CuentaContrato = a.Vkont,
                                Contrato = a.Vertrag,
                                Tarifa = a.Tariftyp,
                                TarifaDescripcion = a.TariftypDesc,
                                UnidadLectura = a.Ableinh,
                                Porcion = a.Portion,
                                NumeroMedidor = a.Sernr,
                                PuntoSuministro = a.Vstelle,
                                ObjetoConexion = a.Haus,
                                Empresa = a.Opbuk,
                                Instalacion = a.Anlage,
                                Direccion = a.Address
                            });
                        }
                    }
                }
                catch (Exception ex)
                {
                    _statusCode = INTERNAL_ERROR_500;
                    _log.Err(ex);
                }
            }

            return dataList;
        }
    }
}
