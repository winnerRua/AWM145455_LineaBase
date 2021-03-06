using GrupoCoen.Corporativo.Libraries.ConexionBD;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using WSREGAWM.Entities;
using WSREGAWM.Helpers;
using WSREGAWM.Services;
using WSREGPROXY.Helpers;
using WSREGPROXY;
using System.Data;
using WSREGPROXY.Entities;
using WSREGPROXY.Services;
using WSSoapProxy.Models;
using System.Xml.Linq;
using Newtonsoft.Json;
using System.Net;
using System.IO;
using System.Linq;

namespace WSREGAWM
{
    public class AWMCore
    {
        UtileriasProxy utiProxy = new UtileriasProxy();
        Transacciones transaccion = new Transacciones();

        dynamic responseRemesador;
        WorkFlows oWF = new WorkFlows();
        string agency_code = string.Empty;
        string sub_agent_user = string.Empty;
        string oError;

        public dynamic beginWorkflow(string Request, IConfiguration _config, string Key, int partnerID)
        {
            dynamic respuesta = new JObject();
            try
            {
                InfoSGR infoSGR = new InfoSGR();
                string tokenSGR = "";
                dynamic oRespuesta;
                int oPais = 0;
                int oSGR = -1;
                bool country = false;
                JObject internalObject = new JObject();
                string request = string.Empty;
                UtileriasAWM miUtilerias = new UtileriasAWM();
                ConexionBDAWM miConexionBD = new ConexionBDAWM();
                Proxy prox = new Proxy();

                var mijObject = JObject.Parse(Request);
                var CodigoAgencia = miUtilerias.getPropertyValue("header.agency_code", mijObject);
                var Producto = miUtilerias.getPropertyValue("header.operation_product_id", mijObject);
                oPais = int.Parse(miUtilerias.getPropertyValue("header.originating_country_id", mijObject));
                var nodo = miUtilerias.encontrarValorNodo(mijObject, "operation_id");

                if (!string.IsNullOrEmpty(CodigoAgencia))
                {
                    miUtilerias.ObtenerValoresAgencia(_config, CodigoAgencia, Producto, ref mijObject);
                }

                oWF = miConexionBD.obtenerWorkFlow(_config,  nodo, Producto, int.Parse(miUtilerias.encontrarValorNodo(mijObject, "remesador_id")));
                utiProxy.SaveLog(oWF.NombreWorkFlow, "1", Key, 1, _config);

                switch (oWF.NombreWorkFlow)
                {
                    case "SENDMONEYSTORE":
                        var send = ejecutarActividades(oWF.Operaciones, _config, Request, int.Parse(miUtilerias.getPropertyValue("header.remesador_id", mijObject)), Key);
                        utiProxy.SaveLog(send, "1", Key, 1, _config);
                        //if (!string.IsNullOrEmpty(Convert.ToString(send)))
                        //{
                        //    utiProxy.SaveLog(send, "1", Key, 1, _config);
                        //    return send;
                        //}

                        if (oWF.ConsumeAutorizador)
                        {
                            //Consumir autorizador
                            var conAuto = miUtilerias.autorizadorRest(Request, Key, _config);
                            utiProxy.SaveLog(conAuto, "1", Key, 1, _config);
                            var validar = miUtilerias.encontrarValorNodo(conAuto, "Fault");//Debería ser igual ya que autorizador devuelve un JSON.
                            if (!string.IsNullOrEmpty(validar))
                            {
                                return validar;
                            }
                        }

                        if (oWF.ConsumeProxy)
                        {
                            //Consumir proxy.
                            //var valProx = prox.ConsumeProxy(Request, Key, _config);
                            //utiProxy.SaveLog(valProx, "1", Key, 1, _config);
                            //var validar = miUtilerias.encontrarValorNodo(valProx, "Fault");
                            //if (!string.IsNullOrEmpty(validar))
                            //{
                            //    return validar;
                            //}
                            string oMoneda = "";
                            string elemento1 = "";
                            //var oDoc = miUtilerias.ObtenerLocacion(_config, oWF.WorkFlowID, ref mijObject);
                            //var elemento = oDoc.Replace("operationID", nodo);
                            //oMoneda = miUtilerias.getPropertyValue(elemento, mijObject);//Se pasa el valor de la locación y busca en la trama.

                            //oRespuesta = JObject.Parse(miUtilerias.mCalculoIVA(Request, oPais, oMoneda, Key, ref oError, _config, oWF.NombreWorkFlow, oWF.WorkFlowID, nodo));
                            //respuesta = oRespuesta;

                            var oDoc = miUtilerias.ObtenerLocacion(_config, oWF.WorkFlowID, ref mijObject);
                            foreach (DataRow prop in oDoc.Rows)
                            {
                                string ValorNodo = prop["ValorNodoRequest"].ToString();
                                if (ValorNodo == "currency_code")
                                {
                                    string NodoRuta = prop["NodoRuta"].ToString();
                                    var elemento = NodoRuta.Replace("operationID", nodo);
                                    elemento1 = elemento;
                                }
                            }
                            oMoneda = miUtilerias.getPropertyValue(elemento1, mijObject);//Se pasa el valor de la locación y busca en la trama.

                            oRespuesta = JObject.Parse(miUtilerias.mCalculoIVA(Request, oPais, oMoneda, Key, ref oError, _config, oWF.NombreWorkFlow, oWF.WorkFlowID, nodo));
                            respuesta = oRespuesta;
                        }

                        if (oWF.GuardaTransaccion)
                        {
                            string tipo = "ENVIO";
                            var ENVIO = miUtilerias.tipoTrans(respuesta, tipo);
                            var trans = transaccion.storeSendMoney(1, ENVIO, _config, oWF.GuardaTransaccion, Key);
                            utiProxy.SaveLog(Convert.ToString(trans), "1", Key, 1, _config);
                            return trans;
                        }

                        if (oWF.ValidaSGR)
                        {

                            string oMensaje = "";
                            string uri;
                            country = mvalidateCountry(Request, "VSM", _config);

                            if (new UtileriasAWM().getAppSettingsKey("SGR_ProcessTransaction", _config) == "true" && country)
                            {
                                uri = new UtileriasAWM().getAppSettingsKey("servicioValidaSGR", _config, "Servicios");
                                infoSGR = new InfoSGR()
                                {
                                    tramaPartner = Request,
                                    keylog = Key,
                                    tokenSGR = ""
                                };
                                dynamic sgr = new UtileriasAWM().consumeServicioValidaSGR(infoSGR, uri, _config);
                                infoSGR.tokenSGR = sgr.GetValue("tokenSGR").ToString();
                                utiProxy.SaveLog(Convert.ToString(sgr), "1", Key, 1, _config);
                            }

                        }
                        if (oWF.AlmacenaSGR)
                        {
                            //-- Validamos el producto SyR
                            string oProducto = new UtileriasAWM().getPropertyValue("header.operation_product_id", JObject.Parse(Request));
                            bool valProducto = true;
                            string uri;
                            //oProducto.Equals(new UtileriasAWM().getAppSettingsKey("KeySYR",_config) )? (oSnF.rq_status.Equals("RELEASE") ? true : false) : true;

                            if (new UtileriasAWM().getAppSettingsKey("SGR_ProcessTransaction", _config) == "true" && country && valProducto)
                            {
                                uri = new UtileriasAWM().getAppSettingsKey("servicioAlmacenaSGR", _config, "Servicios");
                                dynamic sgr = new UtileriasAWM().consumeServicioValidaSGR(infoSGR, uri, _config);
                                string respuestaSGR = sgr.GetValue("respuesta").ToString();
                                utiProxy.SaveLog(respuestaSGR, "1", Key, 1, _config);
                            }
                        }
                        break;

                    case "SENDMONEYVALIDATE":
                        var send1 = ejecutarActividades(oWF.Operaciones, _config, Request, int.Parse(miUtilerias.getPropertyValue("header.remesador_id", mijObject)), Key);
                        utiProxy.SaveLog(send1, "1", Key, 1, _config);
                        //if (!string.IsNullOrEmpty(Convert.ToString(send)))
                        //{
                        //    utiProxy.SaveLog(send, "1", Key, 1, _config);
                        //    return send;
                        //}

                        if (oWF.ConsumeAutorizador)
                        {
                            //Consumir autorizador
                            var conAuto = miUtilerias.autorizadorRest(Request, Key, _config);
                            utiProxy.SaveLog(conAuto, "1", Key, 1, _config);
                            var validar = miUtilerias.encontrarValorNodo(conAuto, "Fault");//Debería ser igual ya que autorizador devuelve un JSON.
                            if (!string.IsNullOrEmpty(validar))
                            {
                                return validar;
                            }
                        }

                        if (oWF.ConsumeProxy)
                        {
                            //Consumir proxy.
                            var valProx = prox.ConsumeProxy(Request, Key, _config);
                            utiProxy.SaveLog(valProx, "1", Key, 1, _config);
                            var validar = miUtilerias.encontrarValorNodo(valProx, "Fault");
                            if (!string.IsNullOrEmpty(validar))
                            {
                                return validar;
                            }
                            string oMoneda = "";
                            var oDoc = miUtilerias.ObtenerLocacion(_config, oWF.WorkFlowID, ref mijObject);
                            var elemento = oDoc.Replace("operationID", nodo);
                            oMoneda = miUtilerias.getPropertyValue(elemento, mijObject);//Se pasa el valor de la locación y busca en la trama.

                            oRespuesta = JObject.Parse(miUtilerias.mCalculoIVA(Request, oPais, oMoneda, Key, ref oError, _config, oWF.NombreWorkFlow, oWF.WorkFlowID, nodo));
                            respuesta = oRespuesta;
                        }

                        if (oWF.ValidaSGR)
                        {

                            string oMensaje = "";
                            string uri;
                            country = mvalidateCountry(Request, "VSM", _config);

                            if (new UtileriasAWM().getAppSettingsKey("SGR_ProcessTransaction", _config) == "true" && country)
                            {
                                uri = new UtileriasAWM().getAppSettingsKey("servicioValidaSGR", _config, "Servicios");
                                infoSGR = new InfoSGR()
                                {
                                    tramaPartner = Request,
                                    keylog = Key,
                                    tokenSGR = ""
                                };
                                dynamic sgr = new UtileriasAWM().consumeServicioValidaSGR(infoSGR, uri, _config);
                                infoSGR.tokenSGR = sgr.GetValue("tokenSGR").ToString();
                                utiProxy.SaveLog(Convert.ToString(sgr), "1", Key, 1, _config);
                            }

                        }
                        if (oWF.AlmacenaSGR)
                        {
                            //-- Validamos el producto SyR
                            string oProducto = new UtileriasAWM().getPropertyValue("header.operation_product_id", JObject.Parse(Request));
                            bool valProducto = true;
                            string uri;
                            //oProducto.Equals(new UtileriasAWM().getAppSettingsKey("KeySYR",_config) )? (oSnF.rq_status.Equals("RELEASE") ? true : false) : true;

                            if (new UtileriasAWM().getAppSettingsKey("SGR_ProcessTransaction", _config) == "true" && country && valProducto)
                            {
                                uri = new UtileriasAWM().getAppSettingsKey("servicioAlmacenaSGR", _config, "Servicios");
                                dynamic sgr = new UtileriasAWM().consumeServicioValidaSGR(infoSGR, uri, _config);
                                string respuestaSGR = sgr.GetValue("respuesta").ToString();
                                utiProxy.SaveLog(respuestaSGR, "1", Key, 1, _config);
                            }
                        }
                        break;

                    case "PAYMONEY":
                        var pay = ejecutarActividades(oWF.Operaciones, _config, Request, int.Parse(miUtilerias.encontrarValorNodo(mijObject, "remesador_id")), Key);
                        utiProxy.SaveLog(pay, "1", Key, 1, _config);
                        if (oWF.ConsumeAutorizador)
                        {
                            //Consumir autorizador
                            var conAuto = miUtilerias.autorizadorRest(Request, Key, _config);
                            utiProxy.SaveLog(conAuto, "1", Key, 1, _config);
                            var validar = miUtilerias.encontrarValorNodo(conAuto, "Fault");//Debería ser igual ya que autorizador devuelve un JSON.
                            if (!string.IsNullOrEmpty(validar))
                            {
                                return conAuto;
                            }
                        }
                        if (oWF.ConsumeProxy)
                        {
                            //Consumir proxy.
                            var valProx = prox.ConsumeProxy(Request, Key, _config);
                            utiProxy.SaveLog(valProx, "1", Key, 1, _config);
                            var validar = miUtilerias.encontrarValorNodo(valProx, "Fault");
                            if (!string.IsNullOrEmpty(validar))
                            {
                                return valProx;
                            }
                        }
                        if (oWF.GuardaTransaccion)
                        {
                            string tipo = "PAGO";
                            var PAGO = miUtilerias.tipoTrans(Request, tipo);
                            var guarda = transaccion.storePayMoney(1, PAGO, _config, oWF.GuardaTransaccion, Key);
                            utiProxy.SaveLog(guarda, "1", Key, 1, _config);
                            return guarda;
                        }
                        break;

                    case "FEEINQUIRY":
                        var feeInquiry = ejecutarActividades(oWF.Operaciones, _config, Request, int.Parse(miUtilerias.getPropertyValue("header.remesador_id", mijObject)), Key);
                        utiProxy.SaveLog(feeInquiry, "1", Key, 1, _config);
                        if (oWF.ConsumeAutorizador)
                        {
                            //Consumir autorizador
                            var conAuto = miUtilerias.autorizadorRest(Request, Key, _config);
                            utiProxy.SaveLog(conAuto, "1", Key, 1, _config);
                            var validar = miUtilerias.encontrarValorNodo(conAuto, "Fault");//Debería ser igual ya que autorizador devuelve un JSON.
                            if (!string.IsNullOrEmpty(validar))
                            {
                                return conAuto;
                            }
                        }
                        if (oWF.ConsumeProxy)
                        {
                            //Consumir proxy.
                            //var valProx = prox.ConsumeProxy(Request, Key, _config);
                            var valProx = "{\"response\":{\"fee-inquiry-reply\":{\"instant_notification\":{\"addl_service_charges\":\"010300002010030100103MSG02010030101103FEE120419501304195096200104170002010030320099200101C02112JD00000819\"},\"financials\":{\"taxes\":{\"tax_rate\":\"0\",\"municipal_tax\":\"0\",\"state_tax\":\"0\",\"county_tax\":\"234\",\"tax_worksheet\":null},\"originators_principal_amount\":\"10000\",\"destination_principal_amount\":\"10000\",\"gross_total_amount\":\"12184\",\"plus_charges_amount\":\"0\",\"pay_amount\":\"10000\",\"charges\":\"1950\",\"tolls\":\"0\",\"originating_currency_principal\":null,\"canadian_dollar_exchange_fee\":\"0\",\"message_charge\":\"0\"},\"promotions\":{\"promo_code_description\":null,\"promo_sequence_no\":\"0\",\"promo_name\":null,\"promo_discount_amount\":\"0\"},\"payment_details\":{\"exchange_rate\":\"1.0000000\",\"exchange_rate_tracs\":\"7.79\"},\"delivery_services\":{\"messsagesms\":{\"msmsservice\":\"N\",\"msmsservice_charges\":\"0\",\"msmsservice_tax\":\"0\"}},\"fee_inquiry_message\":{\"base_message_charge\":\"1700\",\"base_message_limit\":\"0\",\"incremental_message_charge\":\"200\",\"incremental_message_limit\":\"0\"}}}}";
                            //utiProxy.SaveLog(valProx, "1", Key, 1, _config);
                            //var validar = miUtilerias.encontrarValorNodo(valProx, "Fault");
                            //if (!string.IsNullOrEmpty(validar))
                            //{
                            //    return validar;
                            //}
                            //oRespuesta = miUtilerias.mEnvioProxy(oTramaWU, oServicio, oOwner, ref oError, logkey);
                            //oRespuesta = "<?xml version=\"1.0\" encoding=\"utf-8\"?><soapenv:Envelope xmlns:soapenc=\"http://schemas.xmlsoap.org/soap/encoding/\" xmlns:soapenv=\"http://schemas.xmlsoap.org/soap/envelope/\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"><soapenv:Body><xrsi:receive-money-search-reply xmlns:xrsi=\"http://www.westernunion.com/schema/xrsi\"><instant_notification><addl_service_charges /></instant_notification><payment_transactions><payment_transaction><sender><name name_type=\"D\"><first_name>LYRIS</first_name><last_name>MICHELLE</last_name></name><address><city>LODI</city><state>NJ</state><country_code><iso_code><country_code/><currency_code/></iso_code></country_code><state_zip>07644</state_zip><street>DECCAN ROAD</street></address><contact_phone>6305525252</contact_phone><mobile_phone><phone_number /></mobile_phone><mobile_details /></sender><receiver><name name_type=\"D\"><first_name>HECTOR</first_name><last_name>SILVA</last_name></name><address><country_code><iso_code /></country_code></address><preferred_customer><account_nbr /></preferred_customer><mobile_phone><phone_number><national_number>8975658194</national_number></phone_number></mobile_phone><mobile_details><number>8975658194</number></mobile_details></receiver><financials><taxes><tax_worksheet /></taxes><gross_total_amount>25800</gross_total_amount><pay_amount>25000</pay_amount><principal_amount>25000</principal_amount><charges>800</charges></financials><payment_details><expected_payout_location><state_code /><city /></expected_payout_location><destination_country_currency><iso_code><country_code>NI</country_code><currency_code>USD</currency_code></iso_code></destination_country_currency><originating_country_currency><iso_code><country_code>US</country_code><currency_code>USD</currency_code></iso_code></originating_country_currency><originating_city>CAMP HILLPA1</originating_city><transaction_type>WMF</transaction_type><exchange_rate>1.0000</exchange_rate><original_destination_country_currency><iso_code><country_code>NI</country_code><currency_code>USD</currency_code></iso_code></original_destination_country_currency></payment_details><filing_date>08-03-16 </filing_date><filing_time>0557A EDT</filing_time><money_transfer_key>1585725697</money_transfer_key><pay_status_description>W/C</pay_status_description><mtcn>4930645050</mtcn><new_mtcn>1621684930645050</new_mtcn><fusion><fusion_status>W/C</fusion_status><account_number /></fusion><wu_network_agent_indicator /></payment_transaction></payment_transactions><delivery_services><messsagesms><msmsservice>N</msmsservice><msmsservice_charges>0</msmsservice_charges><msmsservice_tax>0</msmsservice_tax></messsagesms></delivery_services><misc_buffer /></xrsi:receive-money-search-reply></soapenv:Body></soapenv:Envelope>";

                            //se inserta el paquete en la actividad Complete
                            string oMoneda = "";
                            string elemento1 = "";
                            var oDoc = miUtilerias.ObtenerLocacion(_config, oWF.WorkFlowID, ref mijObject);
                            foreach (DataRow prop in oDoc.Rows)
                            {
                                string ValorNodo = prop["ValorNodoRequest"].ToString();
                                if (ValorNodo == "currency_code")
                                {
                                    string NodoRuta = prop["NodoRuta"].ToString();
                                    var elemento = NodoRuta.Replace("operationID", nodo);
                                    elemento1 = elemento;
                                }
                            }
                            oMoneda = miUtilerias.getPropertyValue(elemento1, mijObject);//Se pasa el valor de la locación y busca en la trama.

                            oRespuesta = JObject.Parse(miUtilerias.mCalculoIVA(Request, oPais, oMoneda, Key, ref oError, _config, oWF.NombreWorkFlow, oWF.WorkFlowID, nodo));
                            respuesta = oRespuesta;
                            //Calculo de cargos SMS
                            //oRespuesta = AddSMSCharges(valProx, Request, oPais, oMoneda, ref oError, oProducto, oOwner, oAgencia, logkey, "E");
                            //NO TOCAR LOS COMENTARIOS DE ESTA SECCIÓN
                        }
                        break;
                    case "REMNOVAL":
                    case "CURRENCYCONVERT":
                        var currency = ejecutarActividades(oWF.Operaciones, _config, Request, int.Parse(miUtilerias.getPropertyValue("header.remesador_id", mijObject)), Key);
                        utiProxy.SaveLog(currency, "1", Key, 1, _config);
                        if (oWF.ConsumeAutorizador)
                        {
                            //Consumir autorizador
                            var conAuto = miUtilerias.autorizadorRest(Request, Key, _config);
                            utiProxy.SaveLog(conAuto, "1", Key, 1, _config);
                            var validar = miUtilerias.encontrarValorNodo(conAuto, "Fault");//Debería ser igual ya que autorizador devuelve un JSON.
                            if (!string.IsNullOrEmpty(validar))
                            {
                                return conAuto;
                            }
                        }
                        if (oWF.ConsumeProxy)
                        {
                            //Consumir proxy.
                            var valProx = prox.ConsumeProxy(Request, Key, _config);
                            utiProxy.SaveLog(valProx, "1", Key, 1, _config);
                            var validar = miUtilerias.encontrarValorNodo(valProx, "Fault");
                            if (!string.IsNullOrEmpty(validar))
                            {
                                return valProx;
                            }
                        }
                        break;
                    case "RECEIPTPRINT":
                        var recibo = miConexionBD.imprimirBoleta(_config, miUtilerias.getPropertyValue("reprint-receipt-request.mtcn", mijObject),
                            miUtilerias.getPropertyValue("reprint-receipt-request.transaction_date", mijObject),
                            miUtilerias.getPropertyValue("reprint-receipt-request.operation_type", mijObject),
                            miUtilerias.getPropertyValue("header.country_code", mijObject), ref oError);
                        utiProxy.SaveLog(recibo, "1", Key, 1, _config);
                        //return recibo;
                        break;

                    default:
                        utiProxy.SaveLog("Error en WorkFlow, no se ejecutó ninguna operación.", "1", Key, 1, _config);
                        //throw new InvalidOperationException("Tipo de operación desconocida!");
                        break;
                }
            }
            catch (Exception ex)
            {
                oError = ex.Message;
            }
            return respuesta;
        }

        internal JObject ejecutarActividades(List<Operacion> actividades, IConfiguration _config, string Request, int remesadorID, string Key)
        {
            //Authorizer auto = new Authorizer();
            Proxy prox = new Proxy();
            UtileriasAWM miUtilerias = new UtileriasAWM();
            var miObject = JObject.Parse(Request);

            foreach (var actividad in actividades)
            {
                string oError = "";
                //Ejecutar operación.
                if (actividad.actConsumeAutorizador)
                {
                    //Consumir autorizador
                    var conAuto = miUtilerias.autorizadorRest(Request, Key, _config);
                    utiProxy.SaveLog(conAuto, "1", Key, 1, _config);
                    var validar = miUtilerias.encontrarValorNodo(conAuto, "Fault");//Debería ser igual ya que autorizador devuelve un JSON.
                    if (!string.IsNullOrEmpty(validar))
                    {
                        return conAuto;
                    }
                }
                if (actividad.actConsumeProxy)
                {
                    //Consumir proxy.
                    var valProx = prox.ConsumeProxy(Request, Key, _config);
                    utiProxy.SaveLog(valProx, "1", Key, 1, _config);
                    var validar = miUtilerias.getPropertyValue("Fault", valProx);
                    if (!string.IsNullOrEmpty(validar))
                    {
                        var responseError = utiProxy.BuildResponse(ACGError.RequestIsNullOrEmpty, "Error!", agency_code, sub_agent_user);
                        miUtilerias.SaveLog(responseError, "1", Key, 1, _config);
                        return responseError;
                        //return valProx;
                    }
                }
            }
            return miObject;
        }
     
        public bool mvalidateCountry(string trama, string type, IConfiguration config)
        {
            string IsoCountry = "";
            bool Country = false;

            UtileriasAWM util = new UtileriasAWM();
            IsoCountry = util.getIsoCountryValidation(trama, type);
            Country = util.validateCountry(IsoCountry, config);

            return Country;
        }
    }
}