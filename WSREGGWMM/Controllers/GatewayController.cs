using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Net.Http.Headers;
using System.Threading.Tasks;
//using WSREGAUTHORIZER;
using WSREGAWM;
using WSREGGWMM.Entities;
using WSREGGWMM.Helpers;
using WSREGGWMM.Services;
using WSREGPROXY;
using WSREGPROXY.Helpers;

namespace WSREGGWMM.Controllers
{
    [Authorize]
    [Route("api/gateway")]
    [ApiController]
    public class GatewayController : ControllerBase
    {
        private readonly IConfiguration config;
        private readonly IPartnerService partnerService;
        public GatewayController(IConfiguration config, IPartnerService partnerService)
        {
            this.config = config;
            this.partnerService = partnerService;
        }

        public static string Name { get; set; }

        [HttpPost("serviceResponse")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<dynamic>>> serviceResponse(dynamic data)
        {
            #region Declaracion de variables
            Utilerias utilerias = new Utilerias();
            UtileriasProxy utlProxy = new UtileriasProxy();
            ValidarNodos validaNodo = new ValidarNodos(config);
            Proxy miProxy = new Proxy();
            string oError = string.Empty;
            int partnerID = 0;
            string strLogKey = utilerias.GenerateToken(20, 0);
            Dictionary<string, dynamic> auxData = new Dictionary<string, dynamic>();
            #endregion

            //utilerias.SaveLog(data, "1", strLogKey, 1, config);
            //try
            //{
            //    auxData = JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(data.ToString());
            //}
            //catch
            //{
            //    utilerias.SaveLog("No se pudo deserializar petición", "1", strLogKey, 1, config);
            //    return new JsonResult(utilerias.ErrorResponse("No se pudo deserializar petición", data, "No se pudo deserializar petición"));
            //}

            //if (auxData.Count == 0)
            //{
            //    utilerias.SaveLog("El body de la peticion no puede ser nulo", "1", strLogKey, 1, config);
            //    return new JsonResult(utilerias.ErrorResponse("El body de la peticion no puede ser nulo", data, "El body de la peticion no puede ser nulo"));
            //}

            //if (utlProxy.getPropertyValue("header.agency_id", JObject.Parse(data.ToString())) != null)
            //{
            //    partnerID = new ConexionBD().GetPartnerUsuarioID(config, ref oError, Convert.ToInt32(utlProxy.getPropertyValue("header.agency_id", JObject.Parse(data.ToString()))));
            //}
            //else
            //{
            //    partnerID = new ConexionBD().GetPartnerUsuarioID(config, ref oError, 0, (utlProxy.getPropertyValue("header.agency_code", JObject.Parse(data.ToString()))));
            //}

            //if (oError != "" || partnerID == 0)
            //{
            //    return new JsonResult(utilerias.ErrorResponse(oError, data, oError));
            //}
            //List<NodoExcepcion> nodosInvalidos = new List<NodoExcepcion>();
            //var result = validaNodo.validaNodos(Convert.ToString(data),
            //    strLogKey,
            //    utlProxy.getPropertyValue("header.operation_id", JObject.Parse(data.ToString())),
            //    partnerID,
            //    utlProxy.getPropertyValue("header.operation_product_id", JObject.Parse(data.ToString())),
            //    Convert.ToInt32(utlProxy.getPropertyValue("header.remesador_id", JObject.Parse(data.ToString())) ?? "0"),
            //    ref nodosInvalidos);

            //if (!string.IsNullOrEmpty(result))
            //{
            //    return new JsonResult(utilerias.ErrorResponse(result, data, nodosInvalidos));
            //}
            ////SE VA A CONSULTAR LAS CREDENCIALES DE FORMA TRADICIONAL
            //if (new ConexionBD().ValidaCredencialesTracicional(Convert.ToInt32(utlProxy.getPropertyValue("header.remesador_id", JObject.Parse(data.ToString()))), utlProxy.getPropertyValue("header.operation_product_id", JObject.Parse(data.ToString())), ref oError, config))
            //{
            //    var paisID = new ConexionBD().get_paisID(config, utlProxy.getPropertyValue("header.country_code", JObject.Parse(data.ToString())), ref oError);
            //    var CredencialesValidas = new Utilerias().ValidateCredentials(config,
            //        utlProxy.getPropertyValue("header.user", JObject.Parse(data.ToString())),
            //        utlProxy.getPropertyValue("header.password", JObject.Parse(data.ToString())),
            //        Convert.ToInt32(utlProxy.getPropertyValue("header.agency_id", JObject.Parse(data.ToString()))),
            //        paisID,
            //        utlProxy.getPropertyValue("header.country_code", JObject.Parse(data.ToString())),
            //        utlProxy.getPropertyValue("header.operation_product_id", JObject.Parse(data.ToString())),
            //        "",
            //        ref oError);

            //    if (!oError.Equals(string.Empty))
            //    {
            //        utilerias.SaveLog("Error:" + oError, "1", strLogKey, 1, config);
            //        return new CustomResult(oError, StatusCodes.Status500InternalServerError);
            //    }

            //    if (!CredencialesValidas)
            //    {
            //        utilerias.SaveLog("Las credenciales no son correctas.", "1", strLogKey, 1, config);
            //        return new JsonResult(utilerias.ErrorResponse("Credenciales invlaidas", data, "Las credenciales no son correctas."));
            //    }
            //    else
            //    {
            //        var tokenCredenciales = await partnerService.Authenticate(config,
            //                    utlProxy.getPropertyValue("header.user", JObject.Parse(data.ToString())),
            //                    utlProxy.getPropertyValue("header.password", JObject.Parse(data.ToString())),
            //                    Convert.ToInt32(utlProxy.getPropertyValue("header.agency_id", JObject.Parse(data.ToString()))), "",
            //                    utlProxy.getPropertyValue("header.operation_product_id", JObject.Parse(data.ToString())),
            //                    utlProxy.getPropertyValue("header.country_code", JObject.Parse(data.ToString())));

            //        utilerias.SaveLog("tokenCredencialesTradicional" + tokenCredenciales, "1", strLogKey, 1, config);
            //    }
            //}
            //var resultado = miProxy.ConsumeProxy(data.ToString(), strLogKey, config);
            var resultado1 = new AWMCore().beginWorkflow(data.ToString(), config, strLogKey, partnerID);//Consumir WorkFlows acá.
            //if (string.IsNullOrEmpty(result.ToString()))
            //    return new CustomResult(result, StatusCodes.Status500InternalServerError);

            return new CustomResult(resultado1, StatusCodes.Status200OK);
        }

        [AllowAnonymous]
        [HttpPost("authenticate")]
        public async Task<ActionResult<IEnumerable<dynamic>>> AuthenticateAsync(dynamic data)
        {
            if (!Request.Headers.ContainsKey("Authorization"))
                return new CustomResult("Debe enviar las credenciales para conectarse al servicio", StatusCodes.Status401Unauthorized);

            Dictionary<string, string> auxData = JsonConvert.DeserializeObject<Dictionary<string, string>>(data.ToString());

            if (auxData.Count == 0)
                //return new CustomResult("El body de la peticion no puede ser nulo", StatusCodes.Status400BadRequest);
                return StatusCode(StatusCodes.Status500InternalServerError, "El body de la peticion no puede ser nulo");
            if (string.IsNullOrEmpty(auxData["agencyCode"]))
                return new CustomResult("Debe enviar el codigo de la agencia", StatusCodes.Status400BadRequest);

            var authHeader = AuthenticationHeaderValue.Parse(Request.Headers["Authorization"]);
            var credentialBytes = Convert.FromBase64String(authHeader.Parameter);
            var credentials = System.Text.Encoding.UTF8.GetString(credentialBytes).Split(new[] { ':' }, 2);
            var username = credentials[0];
            var password = credentials[1];

            var result = await partnerService.Authenticate(config, username, password, 0, (auxData["agencyCode"]));

            if (string.IsNullOrEmpty(result))
                return new CustomResult("Las credenciales son incorrectas", StatusCodes.Status400BadRequest);

            return new CustomResult(result, StatusCodes.Status200OK);
        }

        //[AllowAnonymous]
        //[HttpPost("testWF")]
        //public async Task<ActionResult<IEnumerable<dynamic>>> TestWorkFlow(dynamic data)
        //{
        //    string oError = "";
        //    var result = new AWMCore().beginWorkflow(data.ToString(), config, "valor");
        //    return new CustomResult(result, StatusCodes.Status200OK);
        //}


    }
}
