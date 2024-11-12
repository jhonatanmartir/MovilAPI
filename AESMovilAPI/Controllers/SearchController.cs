using AESMovilAPI.DTOs;
using AESMovilAPI.Examples;
using AESMovilAPI.Models;
using AESMovilAPI.Responses;
using AESMovilAPI.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Filters;
using System.Numerics;

namespace AESMovilAPI.Controllers
{
    public class SearchController : BaseController
    {
        private readonly SAPSGCDbContext _db;
        public SearchController(IConfiguration config, SAPSGCDbContext db) : base(config)
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
        /// Consulta información de Cuenta contrato o NIC.
        /// </summary>
        /// <param name="id">Cuenta Contrato, NIC o NPE</param>
        /// <remarks>
        /// **Recomendado usar.**
        /// </remarks>
        /// <returns>Información del cliente</returns>
        /// <response code="200">Correcto</response>
        /// <response code="400">El dato a consultar no es correcto.</response>
        /// <response code="401">Error por token de autorización.</response>
        /// <response code="404">No existe información de Cuenta contrato o NIC.</response>
        /// <response code="500">Ha ocurrido un error faltal en el servicio.</response>
        /// <response code="502">Incidente en el servicio.</response>
        /// <response code="503">Error interno en el proceso de consulta.</response>
        // GET: api/Search
        [HttpGet("api/v1/Search/{id}")]
        public async Task<IActionResult> Get(string id)
        {
            Response<List<SAPSGCResponse>> response = new Response<List<SAPSGCResponse>>();
            BigInteger number;

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
                        id = id.Substring(12, 7);
                    }

                    if (id.Length == 6 || id.Length == 7)
                    {
                        // NIC
                        value = await _db.SAPData.Where(a => a.Nic == int.Parse(id)).ToListAsync();
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
                                UnidadLectura = a.Ableinh,
                                Porcion = a.Portion,
                                NumeroMedidor = a.Sernr,
                                PuntoSuministro = a.Vstelle,
                                ObjetoConexion = a.Haus,
                                Empresa = a.Opbuk,
                                Instalacion = a.Anlage
                            });
                        }
                    }
                }
                catch (Exception ex)
                {

                }
            }

            return dataList;
        }
    }
}
