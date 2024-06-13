using AESMovilAPI.DTOs;
using AESMovilAPI.Examples;
using AESMovilAPI.Models;
using AESMovilAPI.Responses;
using AESMovilAPI.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Swashbuckle.AspNetCore.Filters;
using System.Dynamic;
using System.Numerics;

namespace AESMovilAPI.Controllers
{
    [Route("api/v1/[controller]")]
    public class SAPSGCController : BaseController
    {
        private readonly SAPSGCDbContext _db;
        private readonly HttpClient _client;
        public SAPSGCController(IConfiguration config, SAPSGCDbContext db, HttpClient httpClient) : base(config)
        {
            _db = db;
            _client = httpClient;
        }

        /// <summary>
        /// Consulta información de Cuenta contrato o NIC.
        /// </summary>
        /// <param name="query">Número de Cuenta, NIC o NPE, representado por el objeto <see cref="Query">Query</see></param>
        /// <returns>información de contrato o NIC</returns>
        /// <response code="200">Correcto</response>
        /// <response code="400">El dato a consultar no es correcto.</response>
        /// <response code="401">Error por token de autorización.</response>
        /// <response code="404">No existe información de Cuenta contrato o NIC.</response>
        /// <response code="500">Ha ocurrido un error faltal en el servicio.</response>
        /// <response code="502">Incidente en el servicio.</response>
        /// <response code="503">Error interno en el proceso de consulta.</response>
        // GET: api/SAPSGC
        [SwaggerRequestExample(typeof(Query), typeof(SGCSAPExample))]
        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> GetById(Query query)
        {
            bool fromBD = false;
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

                List<Associations> value = new List<Associations>();

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
                            value = _db.Associations.Where(a => a.Nic == int.Parse(id)).ToList();
                        }
                        else
                        {
                            // NC
                            value = _db.Associations.Where(a => a.Vkont == long.Parse(id)).ToList();
                        }

                        if (value != null && value.Count > 0)
                        {
                            _statusCode = OK_200;

                            List<SAPSGCResponse> list = new List<SAPSGCResponse>();

                            foreach (var a in value)
                            {
                                list.Add(new SAPSGCResponse
                                {
                                    Nic = a.Nic,
                                    NisRad = a.NisRad,
                                    Partner = a.Partner,
                                    Nombre = a.NameFirst,
                                    Apellido = a.NameLast,
                                    CuentaContrato = a.Vkont,
                                    Vertrag = a.Vertrag,
                                    Tariftyp = a.Tariftyp,
                                    Ableinh = a.Ableinh,
                                    Portion = a.Portion,
                                    Sernr = a.Sernr,
                                    Vstelle = a.Vstelle,
                                    Haus = a.Haus,
                                    Opbuk = a.Opbuk
                                });
                            }
                            response = new { Data = list, ErrorCode = "0", ErrorMsg = "Servicio ejecutado con exito" };
                        }
                    }
                    catch (Exception ex)
                    {
                        _statusCode = SERVICE_UNAVAILABLE_503;
                        response = new { Data = new List<SAPSGCResponse>(), ErrorCode = "1", ErrorMsg = ex.Message };
                    }
                }
                else
                {
                    if (id.Length == 24)
                    {
                        id = id.Substring(12, 7);
                    }
                    try
                    {
                        // Send the POST request
                        var responseApi = await _client.GetAsync("https://mcacdv01billing003.azurewebsites.net/api/sapsgc/" + id);

                        // Ensure the request was successful
                        responseApi.EnsureSuccessStatusCode();

                        // Read the response content as a string
                        var responseContent = await responseApi.Content.ReadAsStringAsync();
                        dynamic responseObject = JsonConvert.DeserializeObject<ExpandoObject>(responseContent)!;

                        if (responseObject.success == true)
                        {
                            _statusCode = OK_200;

                            List<SAPSGCResponse> list = new List<SAPSGCResponse>();

                            foreach (var item in responseObject.data)
                            {
                                list.Add(new SAPSGCResponse
                                {
                                    Nic = (int)item.nic,
                                    NisRad = (int)item.nisRad,
                                    Partner = (long)item.partner,
                                    Nombre = item.nombre,
                                    Apellido = item.apellido,
                                    CuentaContrato = (long)item.vkont,
                                    Vertrag = (long)item.vertrag,
                                    Tariftyp = item.tariftyp,
                                    Ableinh = item.ableinh,
                                    Portion = item.portion,
                                    Sernr = item.sernr,
                                    Vstelle = item.vstelle,
                                    Haus = item.haus,
                                    Opbuk = item.opbuk
                                });
                            }

                            response = new { Data = list, ErrorCode = "0", ErrorMsg = "Servicio ejecutado con exito" };
                        }
                    }
                    catch (HttpRequestException ex)
                    {
                        _statusCode = SERVICE_UNAVAILABLE_503;
                        response = new { Data = new List<SAPSGCResponse>(), ErrorCode = "1", ErrorMsg = ex.Message };
                    }
                }
            }

            return Ok(response);
        }
    }
}
