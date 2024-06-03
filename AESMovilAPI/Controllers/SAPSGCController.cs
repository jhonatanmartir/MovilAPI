using AESMovilAPI.Models;
using AESMovilAPI.Responses;
using AESMovilAPI.Utilities;
using Microsoft.AspNetCore.Mvc;
using System.Numerics;

namespace AESMovilAPI.Controllers
{
    [Route("api/v1/[controller]")]
    public class SAPSGCController : BaseController
    {
        private readonly SAPSGCDbContext _db;
        public SAPSGCController(IConfiguration config, SAPSGCDbContext db) : base(config)
        {
            _db = db;
        }

        /// <summary>
        /// Consulta información de Cuenta contrato o NIC.
        /// </summary>
        /// <param name="id">Número de Cuenta o NIC.</param>
        /// <returns>información de contrato o NIC</returns>
        /// <response code="200">Correcto</response>
        /// <response code="400">El dato a consultar no es correcto.</response>
        /// <response code="401">Error por token de autorización.</response>
        /// <response code="404">No existe información de Cuenta contrato o NIC.</response>
        /// <response code="500">Incidente en el servicio.</response>
        /// <response code="503">Error interno en el proceso de consulta.</response>
        // GET: api/SAPSGC/2222222
        [HttpGet("{id}")]
        public IActionResult GetById(string id)
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

                List<Associations> value = new List<Associations>();

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
                                Vkont = a.Vkont,
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

                        response.Data = list;
                    }
                }
                catch (Exception ex)
                {
                    _statusCode = SERVICE_UNAVAILABLE_503;
                    response.Message = ex.Message;
                }
            }

            return GetResponse(response);
        }
    }
}
