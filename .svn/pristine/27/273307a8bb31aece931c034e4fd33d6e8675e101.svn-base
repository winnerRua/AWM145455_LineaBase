using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using ProxySGR;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using WSREGAWM.Services;

namespace WSREGAWM.Helpers
{
    class AsyncStoreSGR
    {

        //Parameters
        string _trama = "";
        string _xmlRespuesta = "";
        string _wtipo = "";
        int _idPaisOrigen;
        string _logkey = "";
        string _mensaje = "";
        string _token = "";
        IConfiguration _config;
        //private ControlService.ControlEntities db;
        string _port = "";
        string _serviceMode = "";
        RequestSGR _request;

        public AsyncStoreSGR(IConfiguration confing, string trama, string xmlRespuesta, string wtipo, int idPaisOrigen, string logkey, string token)
        {
            try
            {
                _trama = trama;
                _xmlRespuesta = xmlRespuesta;
                _wtipo = wtipo;
                _idPaisOrigen = idPaisOrigen;
                _logkey = logkey;
                _token = token;
                _config = confing;
                //_port = ConfigurationManager.AppSettings["ControlServicePort"];
                //_serviceMode = ConfigurationManager.AppSettings["ControlServiceMode"];
            }
            catch (Exception ex)
            {
                //AirpakCommunicationProxy.SaveLog(_logkey, ex.ToString());
            }
        }

        private delegate void DelegadoStoreSGR();

        public void StoreSGRAsincrono()
        {
            DelegadoStoreSGR delegado = StoreSGR;
            delegado.BeginInvoke(null, null);
        }

        private void StoreSGR()
        {
            RequestSGR request = new RequestSGR();

            if (_wtipo == "SSM")
            {
                request = fillSGRStoreSend(_config, _trama, _xmlRespuesta, _idPaisOrigen, _logkey, ref _mensaje);
            }
            if (_wtipo == "SPM")
            {
                request = fillSGRStorePay(_config, _trama, _xmlRespuesta, _idPaisOrigen, _logkey, ref _mensaje);
            }
            //if (_wtipo == "SQP")
            //{
            //    request = fillSGRStoreQuickPay(_trama, _xmlRespuesta, _idPaisOrigen, _logkey, ref _mensaje);
            //}

            if (!_mensaje.Trim().Equals(""))
            {
                //LogWriter.Write(_mensaje, Category.None, Level.Info, request.Transactionid,
                // request.Subproductcode.Substring(0, 1), request.Isocountry, request.Sucursal,
                //  request.Transactionid);
            }
            else
            {
                request.Token = _token;
                ProxySGRt SGR = new ProxySGRt(request);
                ResponseSGR response = new ResponseSGR();
                //Log de SGR Request
                //LogWriter.Write(SGR.RequestXML, Category.None, Level.Info, request.Transactionid,
                //                request.Subproductcode.Substring(0, 1), request.Isocountry, request.Sucursal,
                //                request.Transactionid);

                if (SGR.StoreTransaccion(ref response))
                {

                    if (response.token == "ERROR")
                    {
                        _mensaje = response.mensaje;
                    }
                    //LogWriter.Write(SGR.ResponseXML, Category.None, Level.Info, request.Transactionid,
                    //request.Subproductcode.Substring(0, 1), request.Isocountry, request.Sucursal,
                    //request.Transactionid);
                }
                else
                {
                    _mensaje = response.mensaje;
                    //LogWriter.Write(SGR.ResponseXML, Category.Fault, Level.Error, request.Transactionid,
                    //      request.Subproductcode.Substring(0, 1), request.Isocountry, request.Sucursal,
                    //      request.Transactionid);
                }
            }
        }
        private RequestSGR fillSGRStoreSend(IConfiguration config, string trama, string xmlRespuesta, int IdPaisOrigen, string logkey, ref string mensaje)
        {
            RequestSGR request = new RequestSGR();

            UtileriasAWM mUtileria = new UtileriasAWM();

            string operation_product_id = mUtileria.getPropertyValue("header.operation_product_id", JObject.Parse(trama)) == "" ? "wu" : mUtileria.getPropertyValue("header.operation_product_id", JObject.Parse(trama));


            XmlDocument doc = new XmlDocument();
            XDocument xmldoc = XDocument.Parse(trama);
            XDocument xmlReply = XDocument.Parse(xmlRespuesta);
            #region mapping data

            int county_tax = Convert.ToInt32(mUtileria.getPropertyValue("financials.taxes.county_tax", JObject.Parse(trama)));
            int municipal_tax = Convert.ToInt32(mUtileria.getPropertyValue("financials.taxes.municipal_tax", JObject.Parse(trama)));
            int state_tax = Convert.ToInt32(mUtileria.getPropertyValue("financials.taxes.state_tax", JObject.Parse(trama)));
            int charges = Convert.ToInt32(mUtileria.getPropertyValue("financials.charges", JObject.Parse(trama)));
            int message_charge = Convert.ToInt32(mUtileria.getPropertyValue("financials.message_charge", JObject.Parse(trama)));
            string ipError = "";
            string ipPais = new ConexionBD().getUrl(IdPaisOrigen, ref ipError, config);

            dynamic tracsURL = string.Format("http://{0}:{1}/{2}/", ipPais, _port, _serviceMode);
            //db = new ControlService.ControlEntities(new Uri(tracsURL));

            //int AgenciaID = int.Parse(header.First().agency_id);
            //var queryagencia = (from a in db.Agencias.Expand("Grupo")
            //                    where a.AgenciaID == AgenciaID
            //                    select a).FirstOrDefault();

            string moneda = mUtileria.getPropertyValue("payment_details.originating_country_currency.oc_currency_code", JObject.Parse(trama));
            string tasa = "1";
            // tasa = GetTasa(trama, "ENVIO", IdPaisOrigen, AgenciaID, header.First().operation_product_id, logkey, ref mensaje).ToString();
            #endregion

            request.Sucursal = mUtileria.getPropertyValue("header.agency_id", JObject.Parse(trama));
            request.Transactionamount = (Convert.ToInt32(mUtileria.getPropertyValue("financials.originators_principal_amount", JObject.Parse(trama))) / 100).ToString();
            request.Transactioncurrency = mUtileria.getPropertyValue("originating_country_currency.iso_code.currency_code", JObject.Parse(trama));
            request.Isocountry = mUtileria.getPropertyValue("originating_country_currency.iso_code.country_code", JObject.Parse(trama));
            request.Transactiontype = new UtileriasAWM().getAppSettingsKey("SGR_NotaDebito", config);

            //Se realiza cambio para que se envie a SGR Simple y Rpaido, y DigitalWU en caso que sean de Sitio Web

            switch (mUtileria.getPropertyValue("header.operation_product_id", JObject.Parse(trama)).ToLower())
            {
                case "syr":
                    request.Description = new UtileriasAWM().getAppSettingsKey("SGR_SendMoneySYR", config);
                    break;
                default:
                    request.Description = new UtileriasAWM().getAppSettingsKey("SGR_SendMoney", config);
                    break;
            }

            request.Subproductcode = new UtileriasAWM().getAppSettingsKey("SGR_SendMoney", config);
            request.Transactioncharges = ((charges + message_charge) / 100).ToString();
            request.Transactiontax = ((county_tax + municipal_tax + state_tax) / 100).ToString();
            request.Transactionid = mUtileria.getPropertyValue("send-money-store-reply.mtcn", JObject.Parse(trama));
            request.Transactiondate = DateTime.Now.ToString("yyyy-MM-dd");
            request.Exchangerateliq = tasa;
            request.Plataform = mUtileria.getAppSettingsKey("SGR_Platform", config);
            //Se aplico el cambio para que los Envios tengan este nuevo tipo de producto WUOB

            //Se realiza cambio para que se envie a SGR Simple y Rpaido, y DigitalWU en caso que sean de Sitio Web
            switch (mUtileria.getPropertyValue("header.operation_product_id", JObject.Parse(trama)).ToLower())
            {
                case "dwu":
                    request.Productcode = new UtileriasAWM().getAppSettingsKey("SGR_ProductCodeDigitalWU", config);
                    break;
                default:
                    request.Productcode = new UtileriasAWM().getAppSettingsKey("SGR_ProductCodeSendMoney", config);
                    break;
            }


            request.Adjustment = "0";
            request.Subagent = "1";
            //queryagencia.Grupo.CodigoGrupo.ToString();
            request.Token = "";
            request.expirationdate = "";

            return request;
        }

        private RequestSGR fillSGRStorePay(IConfiguration config, string trama, string xmlRespuesta, int IdPaisOrigen, string logkey, ref string mensaje)
        {
            RequestSGR request = new RequestSGR();
            UtileriasAWM mUtileria = new UtileriasAWM();
            #region mapping data
            string _product = string.IsNullOrEmpty(mUtileria.getPropertyValue("header.operation_product_id", JObject.Parse(trama))) ? new UtileriasAWM().getAppSettingsKey("SGR_ProductCode", config) : mUtileria.getPropertyValue("header.operation_product_id", JObject.Parse(trama));
            //_product = header.FirstOrDefault().product_id.Trim().Equals(ConfigurationManager.AppSettings["KeySYR"].Trim().ToString()) ? ConfigurationManager.AppSettings["SGR_ProductCode"] : header.FirstOrDefault().product_id;
            //_product = _product == "" ? System.Configuration.ConfigurationManager.AppSettings["SGR_ProductCode"] : _product;

            //Se realiza cambio para que se envie a SGR Simple y Rpaido, y DigitalWU en caso que sean de Sitio Web
            switch (mUtileria.getPropertyValue("header.operation_product_id", JObject.Parse(trama)).ToLower())
            {
                case "dwu":
                    _product = new UtileriasAWM().getAppSettingsKey("SGR_ProductCodeDigitalWU", config);
                    break;
                default:
                    _product = mUtileria.getPropertyValue("header.operation_product_id", JObject.Parse(trama)).ToLower().Trim().Equals(new UtileriasAWM().getAppSettingsKey("KeySYR", config).Trim().ToString()) ? new UtileriasAWM().getAppSettingsKey("SGR_ProductCode", config) : mUtileria.getPropertyValue("header.operation_product_id", JObject.Parse(trama));
                    break;
            }
            string ipError = "";
            string ipPais = new ConexionBD().getUrl(IdPaisOrigen, ref ipError, config);

            dynamic tracsURL = string.Format("http://{0}:{1}/{2}/", ipPais, _port, _serviceMode);
            //db = new ControlService.ControlEntities(new Uri(tracsURL));

            //int AgenciaID = int.Parse(header.First().agency_id);
            //var queryagencia = (from a in db.Agencias.Expand("Grupo")
            //                    where a.AgenciaID == AgenciaID
            //                    select a).FirstOrDefault();

            string tasa = "1";
            //tasa = GetTasa(trama, "PAGO", IdPaisOrigen, AgenciaID, header.FirstOrDefault().product_id, logkey, ref mensaje).ToString();
            #endregion

            request.Sucursal = mUtileria.getPropertyValue("header.agency_id", JObject.Parse(trama));
            request.Transactionamount = (Convert.ToInt32(mUtileria.getPropertyValue("financials.principal_amount", JObject.Parse(trama))) / 100).ToString();
            request.Transactioncurrency = mUtileria.getPropertyValue("payment_details.destination_country_currency.iso_code.currency_code", JObject.Parse(trama));
            request.Isocountry = mUtileria.getPropertyValue("payment_details.destination_country_currency.iso_code.country_code", JObject.Parse(trama));
            request.Transactiontype = new UtileriasAWM().getAppSettingsKey("SGR_NotaCredito", config);


            //Se realiza cambio para que se envie a SGR diferente descripcion
            switch (mUtileria.getPropertyValue("header.operation_product_id", JObject.Parse(trama)).ToLower())
            {
                case "syr":
                    request.Description = new UtileriasAWM().getAppSettingsKey("SGR_ReceiveMoneySYR", config);

                    break;
                default:
                    request.Description = new UtileriasAWM().getAppSettingsKey("SGR_ReceiveMoney", config);

                    break;
            }

            request.Subproductcode = new UtileriasAWM().getAppSettingsKey("SGR_ReceiveMoney", config);

            request.Transactioncharges = "0";
            request.Transactiontax = "0";
            request.Transactionid = mUtileria.getPropertyValue("receive-money-pay-reply.mtcn", JObject.Parse(trama));
            request.Transactiondate = DateTime.Now.ToString("yyyy-MM-dd");
            request.Exchangerateliq = tasa;
            request.Plataform = new UtileriasAWM().getAppSettingsKey("SGR_Platform", config);

            //request.Productcode = System.Configuration.ConfigurationManager.AppSettings["SGR_ProductCode"]; 
            request.Productcode = _product.ToUpper();
            request.Adjustment = "0";
            request.Subagent = "";
            //queryagencia.Grupo.CodigoGrupo.ToString();
            request.Token = "";
            request.expirationdate = "";

            return request;
        }
    }
}
