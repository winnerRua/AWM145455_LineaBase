using GrupoCoen.Corporativo.Libraries.ConexionBD;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using System;
using System.Globalization;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WSREGPROXY.Helpers;
using WSREGPROXY.Services;
using System.Net;
using System.IO;
using ProxySGR;
using System.Xml;
using System.Xml.Linq;
using WSREGAWM.Services;
using System.Data;
using WSREGAWM.Entities;
using System.Data.SqlClient;
using Newtonsoft.Json;
using static System.Net.Mime.MediaTypeNames;

namespace WSREGAWM.Helpers
{
    public class UtileriasAWM
    {
        MSSQLConnection oCon;
        string oCS = String.Empty;
        string oError;

        WorkFlows oWF = new WorkFlows();
        public void ObtenerValoresAgencia(IConfiguration _config, string CodigoAgencia, string Producto, ref JObject jObjeto)
        {
            DataSet oDT = new DataSet();
            DataTable oTable = new DataTable();
            bool result = false;

            try
            {
                oCS = getAppSettingsKey("dbalias", _config);

                oCon = new MSSQLConnection(oCS);

                List<Parametros> oParam = new List<Parametros>();
                oParam.Add(new Parametros("@AgencyCode", CodigoAgencia, Parametros.SType.VarChar));
                oParam.Add(new Parametros("@ProductID", Producto, Parametros.SType.VarChar));

                try
                {
                    result = oCon.executeSP("[remesadores].[sp_select_GWMMValoresAgencia]", oParam, MSSQLConnection.ReturnTypes.Dataset);
                }
                catch (SqlException oExQ)
                {
                    oError = oExQ.Message;
                }
                catch (Exception oEx)
                {
                    oError = oEx.Message;
                }
                finally
                {
                    oCon.closeConnection();
                }

                if (!result)
                    throw new Exception("No se pudo conectar a la base de datos");
                
                oDT = oCon.getDataset();

                if (oDT.Tables.Count == 0)
                {
                    throw new Exception("La busqueda no obtuvo ningún dato de Usuario-Agencia");
                }

                oTable = oDT.Tables[0];

                if (oTable.Rows.Count == 0)
                {
                    throw new Exception("La busqueda no obtuvo ningún dato de Usuario-Agencia-Tabla");
                }
                JObject header = (JObject)jObjeto["header"];

                foreach (DataRow prop in oTable.Rows)
                {
                    header.Add("user", prop["user1"]?.ToString() ?? "");
                    header.Add("counter_id", prop["CounterID"]?.ToString() ?? "");
                    header.Add("agency_account", prop["CuentaML"]?.ToString() ?? "");
                    header.Add("password", prop["password1"]?.ToString() ?? "");
                    header.Add("terminal_id", prop["TerminalID"]?.ToString() ?? "");
                }
                jObjeto["header"] = header;
            }
            catch (Exception ex)
            {
                oError = ex.Message;
            }
        }

        internal dynamic ObtenerLocacion(IConfiguration _config, int workflowID, ref JObject jObjeto)
        {
            DataSet oDT = new DataSet();
            DataTable oTable = new DataTable();
            bool result = false;
            string final = "";

            try
            {
                oCS = getAppSettingsKey("dbalias", _config);

                oCon = new MSSQLConnection(oCS);

                List<Parametros> oParam = new List<Parametros>();
                oParam.Add(new Parametros("@WFOperacion", workflowID, Parametros.SType.VarChar));

                try
                {
                    result = oCon.executeSP("[remesadores].[sp_select_GWMMObtenerLocacion]", oParam, MSSQLConnection.ReturnTypes.Dataset);
                }
                catch (SqlException oExQ)
                {
                    oError = oExQ.Message;
                }
                catch (Exception oEx)
                {
                    oError = oEx.Message;
                }
                finally
                {
                    oCon.closeConnection();
                }

                if (!result)
                    throw new Exception("No se pudo conectar a la base de datos");

                oDT = oCon.getDataset();

                if (oDT.Tables.Count == 0)
                {
                    throw new Exception("La busqueda no obtuvo ningún dato de Usuario-Agencia");
                }

                oTable = oDT.Tables[0];
                var oTable2 = oDT.Tables[1];
                oTable.Merge(oTable2);

                if (oTable.Rows.Count == 0)
                {
                    throw new Exception("La busqueda no obtuvo ningún dato de Usuario-Agencia-Tabla");
                }
                
                return oTable;
            }
            catch (Exception ex)
            {
                oError = ex.Message;
            }
            return final;
        }

        internal string encontrarValorNodo(JObject jObject, string nodo)
        {
            string value = string.Empty;
            bool isValueFinded = false;

            foreach (var jitem in jObject)
            {
                if (isValueFinded)
                    break;

                if (jitem.Value.Type == JTokenType.Object)
                {
                    if (jitem.Key.ToString() == nodo)
                    {
                        value = jitem.Value.ToString();
                        isValueFinded = true;
                        break;
                    }
                    var jSubObject = (JObject)jitem.Value;
                    foreach (var jsubitem in jSubObject)
                    {
                        if (jsubitem.Key.ToString() == nodo)
                        {
                            value = jsubitem.Value.ToString();
                            isValueFinded = true;
                            break;
                        }
                        else if (jsubitem.Value.Type == JTokenType.Object)
                        {
                            value = encontrarValorNodo((JObject)jsubitem.Value, nodo);
                            isValueFinded = true;
                            break;
                        }

                    }
                }
                else
                {
                    if (jitem.Key.ToString() == nodo)
                    {
                        value = jitem.Value.ToString();
                        isValueFinded = true;
                        break;
                    }
                }
            }
            return value;
        }
        internal string encontrarValor(JObject jObject, string nodo)
        {
            string value = string.Empty;
            bool isValueFinded = false;

            foreach (var jitem in jObject)
            {
                if (isValueFinded)
                    break;

                if (jitem.Value.Type == JTokenType.Object)
                {
                    if (jitem.Key.ToString() == nodo)
                    {
                        value = jitem.Value.ToString();
                        isValueFinded = true;
                        break;
                    }
                    var jSubObject = (JObject)jitem.Value;
                    foreach (var jsubitem in jSubObject)
                    {
                        if (jsubitem.Key.ToString() == nodo)
                        {
                            value = jsubitem.Value.ToString();
                            isValueFinded = true;
                            break;
                        }
                        else if (jsubitem.Value.Type == JTokenType.Object)
                        {
                            value = encontrarValorNodo((JObject)jsubitem.Value, nodo);
                            isValueFinded = true;
                            break;
                        }

                    }
                }
                else
                {
                    if (jitem.Key.ToString() == nodo)
                    {
                        value = jitem.Value.ToString();
                        isValueFinded = true;
                        break;
                    }
                }
            }
            return value;
        }
        internal string getAppSettingsKey(string KeyName, IConfiguration _config, string keySection = "AppSettings")
        {
            try
            {
                if (string.IsNullOrEmpty(KeyName))
                    throw new Exception();
                return _config.GetValue<string>(keySection + ":" + KeyName);
            }
            catch
            {
                return string.Empty;
            }
        }

        internal string getServicesKey(string KeyName, IConfiguration _config, string keySection = "Servicios")
        {
            try
            {
                if (string.IsNullOrEmpty(KeyName))
                    throw new Exception();
                return _config.GetValue<string>(keySection + ":" + KeyName);
            }
            catch
            {
                return string.Empty;
            }
        }
        internal string getPropertyValue(string compoundProperty, JObject myObject)
        {
            string[] bits = compoundProperty.Split('.');

            for (int i = 0; i < bits.Length - 1; i++)
            {
                var auxObject = myObject.Properties().Where(prop => prop.Name == bits[i]).FirstOrDefault()?.Value?.ToString();
                if (!string.IsNullOrEmpty(auxObject))
                    myObject = JObject.Parse(auxObject);
            }

            return myObject.Properties().Where(prop => prop.Name == bits[bits.Length - 1]).FirstOrDefault()?.Value?.ToString();
        }

        internal void SaveLog(dynamic pMetadata, string identificadorEvento, string keyEvent, int ComponentID, IConfiguration _config)
        {
            try
            {
                string oError = string.Empty;
                SaveLogDataDelegate.SaveLogDataDelegateAsync(pMetadata, identificadorEvento, keyEvent, ComponentID, _config);
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        internal dynamic autorizadorRest(dynamic trama, string keyLog, IConfiguration _config)
        {
            var url = new UtileriasAWM().getServicesKey("servicioConsumeAutorizador", _config);//La URL debe venir de appsettings //Preguntar como consumir acá el AUTORIZADOR 
            dynamic result = "";
            var request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "POST";
            request.ContentType = "application/json";
            request.Accept = "application/json";

            try
            {
                using (WebResponse response = request.GetResponse())
                {
                    using (Stream strReader = response.GetResponseStream())
                    {
                        using (StreamReader objReader = new StreamReader(strReader))
                        {
                            string responseBody = objReader.ReadToEnd();
                            return responseBody;
                        }
                    }
                }
               //return JObject.Parse(responseBody);
            }
            catch (Exception ex)
            {
                oError = ex.Message;
                throw new Exception(oError);
            }
        }


        internal dynamic tipoTrans(string Request, string tipo)
        {
            string paisValidacion = String.Empty;
            string guardaTransaccion = String.Empty;
            var tasa = 1;
            var mijObject = JObject.Parse(Request);

            if (tipo == "ENVIO")
            {
                Dictionary<dynamic, dynamic> ENVIO = new Dictionary<dynamic, dynamic>();
                ENVIO.Add("PaisValidacion", getPropertyValue("header.country_code", mijObject));
                //ENVIO.Add("tipoTransaccion", miUtilerias.getPropertyValue("header.terminal_id", mijObject));
                //ENVIO.Add("guardaTransaccion", miUtilerias.getPropertyValue("header.country_code", mijObject));
                ENVIO.Add("IdTerminal", getPropertyValue("header.terminal_id", mijObject));
                ENVIO.Add("PaisID", getPropertyValue("header.originating_country_id", mijObject));
                ENVIO.Add("NumeroOperador", getPropertyValue("send-money-store-request.operator", mijObject));
                ENVIO.Add("AgenciaID", getPropertyValue("header.agency_id", mijObject));
                ENVIO.Add("MTCN", getPropertyValue("send-money-store-request.mtcn", mijObject));
                ENVIO.Add("PaisRem", getPropertyValue("send-money-store-request.sender.address.country_code.country_name", mijObject));
                ENVIO.Add("EstadoRem", getPropertyValue("send-money-store-request.sender.address.state", mijObject));
                ENVIO.Add("CiudadRem", getPropertyValue("send-money-store-request.sender.address.city", mijObject));
                ENVIO.Add("PrimerNombreRem", getPropertyValue("send-money-store-request.sender.name.first_name", mijObject));

                ENVIO.Add("PrimerApellidoRem", getPropertyValue("send-money-store-request.sender.name.paternal_name", mijObject));
                ENVIO.Add("SegundoNombreRem", getPropertyValue("send-money-store-request.sender.name.middle_name", mijObject));
                ENVIO.Add("SegundoApellidoRem", getPropertyValue("send-money-store-request.sender.name.maternal_name", mijObject));
                ENVIO.Add("ApellidoCasadaRem", getPropertyValue("send-money-store-request.sender.name.married_name", mijObject));
                ENVIO.Add("Remitente", getPropertyValue("send-money-store-request.sender.name.given_name", mijObject) + " " + getPropertyValue("send-money-store-request.sender.name.paternal_name", mijObject) + " " + getPropertyValue("send-money-store-request.sender.name.maternal_name", mijObject));
                ENVIO.Add("PaisBen", getPropertyValue("send-money-store-request.receiver.address.country_code.country_name", mijObject));
                ENVIO.Add("EstadoBen", getPropertyValue("send-money-store-request.receiver.address.state", mijObject));
                ENVIO.Add("CiudadBen", getPropertyValue("send-money-store-request.receiver.address.city", mijObject));
                ENVIO.Add("PrimerNombreBen", getPropertyValue("send-money-store-request.receiver.name.first_name", mijObject));
                ENVIO.Add("PrimerApellidoBen", getPropertyValue("send-money-store-request.receiver.name.paternal_name", mijObject));

                ENVIO.Add("SegundoNombreBen", getPropertyValue("send-money-store-request.receiver.name.middle_name", mijObject));
                ENVIO.Add("SegundoApellidoBen", getPropertyValue("send-money-store-request.receiver.name.maternal_name", mijObject));
                ENVIO.Add("ApellidoCasadaBen", getPropertyValue("send-money-store-request.receiver.name.married_name", mijObject));
                ENVIO.Add("Beneficiario", getPropertyValue("send-money-store-request.receiver.name.give_name", mijObject) + " " + getPropertyValue("send-money-store-request.receiver.name.paternal_name", mijObject) + " " + getPropertyValue("send-money-store-request.receiver.name.maternal_name", mijObject));
                ENVIO.Add("TipoDocumento", getPropertyValue("send-money-store-request.sender.compliance_details.id_details.id_type", mijObject));
                ENVIO.Add("NumeroDocumento", getPropertyValue("send-money-store-request.sender.compliance_details.id_details.id_number", mijObject));
                ENVIO.Add("EmitidoPor", getPropertyValue("send-money-store-request.sender.compliance_details.id_details.id_country_of_issue", mijObject));
                ENVIO.Add("LugarEmision", getPropertyValue("send-money-store-request.sender.compliance_details.id_details.id_place_of_issue", mijObject));
                ENVIO.Add("FechaEmision", DateTime.ParseExact(getPropertyValue("send-money-store-request.sender.compliance_details.id_issue_date", mijObject), "ddMMyyyy", CultureInfo.InvariantCulture));
                ENVIO.Add("FechaExpiracion", DateTime.ParseExact(getPropertyValue("send-money-store-request.sender.compliance_details.id_expiration_date", mijObject), "ddMMyyyy", CultureInfo.InvariantCulture));

                ENVIO.Add("Nacionalidad", getPropertyValue("send-money-store-request.sender.address.country_code.country_name", mijObject));
                ENVIO.Add("DirLinea1", getPropertyValue("send-money-store-request.sender.address.addr_line1", mijObject));
                ENVIO.Add("DirLinea2", getPropertyValue("send-money-store-request.sender.address.addr_line2", mijObject));
                var indicaciones = getPropertyValue("send-money-store-request.sender.address.addr_line1", mijObject) + " " + getPropertyValue("send-money-store-request.sender.address.addr_line2", mijObject);
                ENVIO.Add("Indicaciones", indicaciones);
                ENVIO.Add("Sexo", getPropertyValue("send-money-store-request.sender.gender", mijObject));
                var estadoCivil = getPropertyValue("send-money-store-request.sender.compliance_details.Marital_Status", mijObject);
                switch (estadoCivil)
                {
                    case "Common-law marriage":
                        estadoCivil = "U";
                        break;

                    case "Married":
                        estadoCivil = "C";
                        break;
                    case "Widow":
                        estadoCivil = "V";
                        break;
                    default:
                        estadoCivil = getPropertyValue("send-money-store-request.sender.compliance_details.Marital_Status", mijObject).ToString().Substring(0, 1);
                        break;
                }
                ENVIO.Add("EstadoCivil", estadoCivil);
                ENVIO.Add("FechaNacimiento", DateTime.ParseExact(getPropertyValue("send-money-store-request.sender.date_of_birth", mijObject), "ddMMyyyy", CultureInfo.InvariantCulture));
                ENVIO.Add("Profesion", getPropertyValue("send-money-store-request.sender.compliance_details.occupation", mijObject));
                ENVIO.Add("LugarTrabajo", getPropertyValue("send-money-store-request.sender.compliance_details.Name_of_Employer_Business", mijObject));
                ENVIO.Add("TipoTelefono", getPropertyValue("send-money-store-request.sender.phone_type", mijObject));

                ENVIO.Add("NumeroTelefono", getPropertyValue("send-money-store-request.sender.contact_phone", mijObject));
                ENVIO.Add("Dato1", getPropertyValue("send-money-store-request.sender.compliance_details.additional_info.source_of_funds", mijObject) + " " + getPropertyValue("send-money-store-request.sender.compliance_details.additional_info.destination_of_funds", mijObject));
                ENVIO.Add("Dato2", getPropertyValue("send-money-store-request.sender.compliance_details.additional_info.type_of_relationship", mijObject) + " " + getPropertyValue("send-money-store-request.sender.compliance_details.additional_info.is_not_final_sender_receiver", mijObject));
                ENVIO.Add("ClienteID", 1);
                ENVIO.Add("CtaAgencia", getPropertyValue("header.agency_account", mijObject));
                ENVIO.Add("CtaBanco", 0);
                var moneda = getPropertyValue("send-money-store-request.payment_details.originating_country_currency.iso_code.currency_code", mijObject);
                if (moneda == "USD")
                {
                    decimal montoPrincipal = Convert.ToDecimal(getPropertyValue("send-money-store-request.financials.originators_principal_amount", mijObject));
                    decimal monto = montoPrincipal / 100;
                    ENVIO.Add("Monto", monto);
                    ENVIO.Add("MontoML", monto * tasa);
                    ENVIO.Add("Tasa", tasa);
                    decimal cargos = Convert.ToDecimal(getPropertyValue("send-money-store-request.financials.charges", mijObject)) / 100;
                    ENVIO.Add("Cargos", cargos);

                    ENVIO.Add("CargosML", cargos * tasa);
                    decimal pCharge = Convert.ToDecimal(getPropertyValue("send-money-store-request.financials.plus_charges_amount", mijObject)) / 100;
                    ENVIO.Add("CargosxEntrega", pCharge);
                    ENVIO.Add("CargosxEntregaML", pCharge * tasa);
                    decimal mCharges = Convert.ToDecimal(getPropertyValue("send-money-store-request.financials.message_charge", mijObject)) / 100;
                    ENVIO.Add("CargosxMensaje", mCharges);
                    ENVIO.Add("CargosxMensajeML", mCharges * tasa);
                    decimal mun_tax = Convert.ToDecimal(getPropertyValue("send-money-store-request.financials.taxes.municipal_tax", mijObject));
                    decimal st_tax = Convert.ToDecimal(getPropertyValue("send-money-store-request.financials.taxes.state_tax", mijObject));
                    decimal coun_tax = Convert.ToDecimal(getPropertyValue("send-money-store-request.financials.taxes.county_tax", mijObject));
                    decimal tax = (mun_tax + st_tax + coun_tax) / 100;
                    ENVIO.Add("Impuesto", tax);

                    decimal impuestoML = tax * tasa;
                    ENVIO.Add("ImpuestoML", impuestoML);
                }
                else
                {
                    decimal montoPrincipal = Convert.ToDecimal(getPropertyValue("send-money-store-request.financials.originators_principal_amount", mijObject));
                    decimal monto = montoPrincipal / 100;
                    ENVIO.Add("Monto", monto / tasa);
                    ENVIO.Add("MontoML", monto);
                    ENVIO.Add("Tasa", tasa);
                    decimal cargos = Convert.ToDecimal(getPropertyValue("send-money-store-request.financials.charges", mijObject)) / 100;
                    ENVIO.Add("Cargos", cargos / tasa);

                    ENVIO.Add("CargosML", cargos);
                    decimal pCharge = Convert.ToDecimal(getPropertyValue("send-money-store-request.financials.plus_charges_amount", mijObject)) / 100;
                    ENVIO.Add("CargosxEntrega", pCharge / tasa);
                    ENVIO.Add("CargosxEntregaML", pCharge);
                    decimal mCharges = Convert.ToDecimal(getPropertyValue("send-money-store-request.financials.message_charge", mijObject)) / 100;
                    ENVIO.Add("CargosxMensaje", mCharges / tasa);
                    ENVIO.Add("CargosxMensajeML", mCharges);
                    double mun_tax = Convert.ToDouble(getPropertyValue("send-money-store-request.financials.taxes.municipal_tax", mijObject));
                    double st_tax = Convert.ToDouble(getPropertyValue("send-money-store-request.financials.taxes.state_tax", mijObject));
                    double coun_tax = Convert.ToDouble(getPropertyValue("send-money-store-request.financials.taxes.county_tax", mijObject));
                    var tax = (mun_tax + st_tax + coun_tax) / 100;
                    ENVIO.Add("Impuesto", tax / tasa);
                    ENVIO.Add("ImpuestoML", tax);
                }

                ENVIO.Add("WUCard", getPropertyValue("send-money-store-request.sender.preferred_customer.account_nbr", mijObject));
                ENVIO.Add("NumFact", getPropertyValue("header.bill_data.number", mijObject));
                ENVIO.Add("NameFact", getPropertyValue("header.bill_data.name", mijObject));

                ENVIO.Add("DirFact", getPropertyValue("header.bill_data.address", mijObject));
                ENVIO.Add("NIT", getPropertyValue("header.bill_data.nit", mijObject));
                ENVIO.Add("Serie", getPropertyValue("header.bill_data.series", mijObject));
                var paisSender = getPropertyValue("send-money-store-request.sender.address.country_code.country_name", mijObject);
                var paisReceiver = getPropertyValue("send-money-store-request.receiver.address.country_code.country_name", mijObject);
                int tipoEnvio = 0;
                if (paisSender == paisReceiver)
                {
                    tipoEnvio = 1;
                }
                else
                {
                    tipoEnvio = 2;
                }
                ENVIO.Add("TipoEnvio", tipoEnvio);
                ENVIO.Add("EstatusAct", "");
                ENVIO.Add("EstatusSet", "");
                ENVIO.Add("UsuarioID", 1);
                ENVIO.Add("FechaEnvio", DateTime.Today);
                ENVIO.Add("Fecha", DateTime.Today);
                ENVIO.Add("FechaCreacion", DateTime.Today);

                return ENVIO;
            }
            else if (tipo == "PAGO")
            {
                Dictionary<dynamic, dynamic> PAGO = new Dictionary<dynamic, dynamic>();
                PAGO.Add("PaisValidacion", getPropertyValue("header.country_code", mijObject));
                //PAGO.Add("tipoTransaccion", miUtilerias.getPropertyValue("header.terminal_id", mijObject));
                //PAGO.Add("guardaTransaccion", miUtilerias.getPropertyValue("header.country_code", mijObject));
                PAGO.Add("IdTerminal", getPropertyValue("header.terminal_id", mijObject));
                PAGO.Add("PaisID", getPropertyValue("header.originating_country_id", mijObject));
                PAGO.Add("NumeroOperador", getPropertyValue("receive-money-pay-request.operator", mijObject));
                PAGO.Add("AgenciaID", getPropertyValue("header.agency_id", mijObject));
                PAGO.Add("MTCN", getPropertyValue("receive-money-pay-request.mtcn", mijObject));
                PAGO.Add("PaisRem", getPropertyValue("header.sender.address.country_name", mijObject));
                PAGO.Add("EstadoRem", getPropertyValue("header.sender.address.state", mijObject));
                PAGO.Add("CiudadRem", getPropertyValue("header.sender.address.city", mijObject));
                PAGO.Add("PrimerNombreRem", getPropertyValue("header.sender.name.first_name", mijObject));

                PAGO.Add("PrimerApellidoRem", getPropertyValue("header.sender.name.paternal_name", mijObject));
                PAGO.Add("SegundoNombreRem", getPropertyValue("header.sender.name.middle_name", mijObject));
                PAGO.Add("SegundoApellidoRem", getPropertyValue("header.sender.name.maternal_name", mijObject));
                PAGO.Add("ApellidoCasadaRem", getPropertyValue("header.sender.name.married_name", mijObject));
                PAGO.Add("Remitente", getPropertyValue("header.sender.name.given_name", mijObject) + " " + getPropertyValue("header.sender.name.paternal_name", mijObject) + " " + getPropertyValue("header.sender.name.maternal_name", mijObject));
                PAGO.Add("PaisBen", getPropertyValue("receive-money-pay-request.receiver.address.country_code.country_name", mijObject));
                PAGO.Add("EstadoBen", getPropertyValue("receive-money-pay-request.receiver.address.state", mijObject));
                PAGO.Add("CiudadBen", getPropertyValue("receive-money-pay-request.receiver.address.city", mijObject));
                PAGO.Add("PrimerNombreBen", getPropertyValue("receive-money-pay-request.receiver.name.first_name", mijObject));
                PAGO.Add("PrimerApellidoBen", getPropertyValue("receive-money-pay-request.receiver.name.paternal_name", mijObject));

                PAGO.Add("SegundoNombreBen", getPropertyValue("receive-money-pay-request.receiver.name.middle_name", mijObject));
                PAGO.Add("SegundoApellidoBen", getPropertyValue("receive-money-pay-request.receiver.name.maternal_name", mijObject));
                PAGO.Add("ApellidoCasadaBen", getPropertyValue("receive-money-pay-request.receiver.name.married_name", mijObject));
                PAGO.Add("Beneficiario", getPropertyValue("receive-money-pay-request.receiver.name.given_name", mijObject) + " " + getPropertyValue("receive-money-pay-request.receiver.name.paternal_name", mijObject) + " " + getPropertyValue("receive-money-pay-request.receiver.name.maternal_name", mijObject));
                PAGO.Add("TipoDocumento", getPropertyValue("receive-money-pay-request.receiver.compliance_details.id_details.id_type", mijObject));
                PAGO.Add("NumeroDocumento", getPropertyValue("receive-money-pay-request.receiver.compliance_details.id_details.id_number", mijObject));
                PAGO.Add("EmitidoPor", getPropertyValue("receive-money-pay-request.receiver.compliance_details.id_details.id_country_of_issue", mijObject));
                PAGO.Add("LugarEmision", getPropertyValue("receive-money-pay-request.receiver.compliance_details.id_details.id_place_of_issue", mijObject));
                PAGO.Add("FechaEmision", DateTime.ParseExact(getPropertyValue("receive-money-pay-request.receiver.compliance_details.id_issue_date", mijObject), "ddMMyyyy", CultureInfo.InvariantCulture));
                PAGO.Add("FechaExpiracion", DateTime.ParseExact(getPropertyValue("receive-money-pay-request.receiver.compliance_details.id_expiration_date", mijObject), "ddMMyyyy", CultureInfo.InvariantCulture));

                PAGO.Add("Nacionalidad", getPropertyValue("receive-money-pay-request.receiver.address.country_code.country_name", mijObject));
                PAGO.Add("DirLinea1", getPropertyValue("receive-money-pay-request.receiver.compliance_details.Current_address.addr_line1", mijObject));
                PAGO.Add("DirLinea2", getPropertyValue("receive-money-pay-request.receiver.compliance_details.Current_address.addr_line2", mijObject));
                var indicaciones = getPropertyValue("receive-money-pay-request.receiver.compliance_details.Current_address.addr_line1", mijObject) + " " + getPropertyValue("receive-money-pay-request.receiver.compliance_details.Current_address.addr_line2", mijObject);
                PAGO.Add("Indicaciones", indicaciones);
                PAGO.Add("Sexo", getPropertyValue("receive-money-pay-request.receiver.compliance_details.Gender", mijObject));
                var estadoCivil = getPropertyValue("receive-money-pay-request.receiver.compliance_details.Marital_Status", mijObject);
                switch (estadoCivil)
                {
                    case "Common-law marriage":
                        estadoCivil = "U";
                        break;

                    case "Married":
                        estadoCivil = "C";
                        break;
                    case "Widow":
                        estadoCivil = "V";
                        break;
                    default:
                        estadoCivil = getPropertyValue("receive-money-pay-request.receiver.compliance_details.Marital_Status", mijObject).ToString().Substring(0, 1);
                        break;
                }
                PAGO.Add("EstadoCivil", estadoCivil);
                PAGO.Add("FechaNacimiento", DateTime.ParseExact(getPropertyValue("receive-money-pay-request.receiver.compliance_details.date_of_birth", mijObject), "ddMMyyyy", CultureInfo.InvariantCulture));
                PAGO.Add("Profesion", getPropertyValue("receive-money-pay-request.receiver.compliance_details.occupation", mijObject));
                PAGO.Add("LugarTrabajo", getPropertyValue("receive-money-pay-request.receiver.compliance_details.Name_of_Employer_Business", mijObject));
                PAGO.Add("TipoTelefono", getPropertyValue("receive-money-pay-request.receiver.phone_type", mijObject));

                PAGO.Add("NumeroTelefono", getPropertyValue("receive-money-pay-request.receiver.compliance_details.contact_phone", mijObject));
                PAGO.Add("Dato1", getPropertyValue("receive-money-pay-request.receiver.compliance_details.additional_info.source_of_funds", mijObject) + " " + getPropertyValue("receive-money-pay-request.receiver.compliance_details.additional_info.destination_of_funds", mijObject));
                PAGO.Add("Dato2", getPropertyValue("receive-money-pay-request.receiver.compliance_details.additional_info.type_of_relationship", mijObject) + " " + getPropertyValue("receive - money - pay - request.receiver.compliance_details.additional_info.age", mijObject));
                PAGO.Add("ClienteID", 1);
                PAGO.Add("CtaAgencia", getPropertyValue("header.agency_code", mijObject));
                PAGO.Add("CtaBanco", 0);
                var moneda = getPropertyValue("receive-money-pay-request.payment_details.destination_country_currency.iso_code.currency_code", mijObject);
                if (moneda == "USD")
                {
                    decimal montoPrincipal = Convert.ToDecimal(getPropertyValue("receive-money-pay-request.financials.principal_amount", mijObject));
                    decimal monto = montoPrincipal / 100;
                    PAGO.Add("Monto", monto);
                    PAGO.Add("MontoML", monto * tasa);
                    PAGO.Add("Tasa", tasa);
                    decimal cargos = Convert.ToDecimal(getPropertyValue("receive-money-pay-request.financials.charges", mijObject)) / 100;
                    PAGO.Add("Cargos", cargos);

                    PAGO.Add("CargosML", cargos * tasa);
                    decimal pCharge = Convert.ToDecimal(getPropertyValue("receive-money-pay-request.financials.charges", mijObject)) / 100;
                    PAGO.Add("CargosxEntrega", pCharge);
                    PAGO.Add("CargosxEntregaML", pCharge * tasa);
                    decimal mCharges = Convert.ToDecimal(getPropertyValue("receive-money-pay-request.financials.message_charge", mijObject)) / 100;
                    PAGO.Add("CargosxMensaje", mCharges);
                    PAGO.Add("CargosxMensajeML", mCharges * tasa);
                    decimal mun_tax = Convert.ToDecimal(getPropertyValue("receive-money-pay-request.financials.taxes.municipal_tax", mijObject));
                    decimal st_tax = Convert.ToDecimal(getPropertyValue("receive-money-pay-request.financials.taxes.state_tax", mijObject));
                    decimal coun_tax = Convert.ToDecimal(getPropertyValue("receive-money-pay-request.financials.taxes.county_tax", mijObject));
                    decimal tax = (mun_tax + st_tax + coun_tax) / 100;
                    PAGO.Add("Impuesto", tax);

                    decimal impuestoML = tax * tasa;
                    PAGO.Add("ImpuestoML", impuestoML);
                }
                else
                {
                    decimal montoPrincipal = Convert.ToDecimal(getPropertyValue("receive-money-pay-request.financials.principal_amount", mijObject));
                    decimal monto = montoPrincipal / 100;
                    PAGO.Add("Monto", monto / tasa);
                    PAGO.Add("MontoML", monto);
                    PAGO.Add("Tasa", tasa);
                    decimal cargos = Convert.ToDecimal(getPropertyValue("receive-money-pay-request.financials.charges", mijObject)) / 100;
                    PAGO.Add("Cargos", cargos / tasa);

                    PAGO.Add("CargosML", cargos);
                    decimal pCharge = Convert.ToDecimal(getPropertyValue("receive-money-pay-request.financials.plus_charges_amount", mijObject)) / 100;
                    PAGO.Add("CargosxEntrega", pCharge / tasa);
                    PAGO.Add("CargosxEntregaML", pCharge);
                    decimal mCharges = Convert.ToDecimal(getPropertyValue("receive-money-pay-request.financials.message_charge", mijObject)) / 100;
                    PAGO.Add("CargosxMensaje", mCharges / tasa);
                    PAGO.Add("CargosxMensajeML", mCharges);
                    double mun_tax = Convert.ToDouble(getPropertyValue("receive-money-pay-request.financials.taxes.municipal_tax", mijObject));
                    double st_tax = Convert.ToDouble(getPropertyValue("receive-money-pay-request.financials.taxes.state_tax", mijObject));
                    double coun_tax = Convert.ToDouble(getPropertyValue("receive-money-pay-request.financials.taxes.county_tax", mijObject));
                    var tax = (mun_tax + st_tax + coun_tax) / 100;
                    PAGO.Add("Impuesto", tax / tasa);
                    PAGO.Add("ImpuestoML", tax);
                }

                PAGO.Add("WUCard", getPropertyValue("receive-money-pay-request.receiver.preferred_customer.account_nbr", mijObject));
                PAGO.Add("NumFact", getPropertyValue("header.bill_data.number", mijObject));
                PAGO.Add("NameFact", getPropertyValue("header.bill_data.name", mijObject));

                PAGO.Add("DirFact", getPropertyValue("header.bill_data.address", mijObject));
                PAGO.Add("NIT", getPropertyValue("header.bill_data.nit", mijObject));
                PAGO.Add("Serie", getPropertyValue("header.bill_data.series", mijObject));
                var paisSender = getPropertyValue("header.sender.address.country_name", mijObject);
                var paisReceiver = getPropertyValue("receive-money-pay-request.receiver.address.country_code.country_name", mijObject);
                int tipoEnvio = 0;
                if (paisSender == paisReceiver)
                {
                    tipoEnvio = 1;
                }
                else
                {
                    tipoEnvio = 2;
                }
                PAGO.Add("TipoEnvio", tipoEnvio);
                PAGO.Add("EstatusAct", "");
                PAGO.Add("EstatusSet", "");
                PAGO.Add("UsuarioID", 1);
                PAGO.Add("FechaEnvio", DateTime.Today);
                PAGO.Add("Fecha", DateTime.Today);
                PAGO.Add("FechaCreacion", DateTime.Today);

                return PAGO;
            }
            return "Operación no ejecutada, revise o comuníquise con el administrador.";
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

     
        //funciones para SGR
        public string getIsoCountryValidation(string trama, string wtipo)
        {
            string country = "";
            if (wtipo == "VSM" || wtipo == "VQP")
            {
                country = getPropertyValue(encontrarValorNodo(JObject.Parse(trama), "operation_id") + "." + "payment_details.originating_country_currency.iso_code.country_code", JObject.Parse(trama)); ;
            }
            if (wtipo == "VPM")
            {
                country = getPropertyValue(encontrarValorNodo(JObject.Parse(trama), "operation_id") + "." + "payment_details.originating_country_currency.iso_code.country_code", JObject.Parse(trama));
            }

            return country;

        }
        //MetodoPara Validar el pais de donde proviene la transaccion y si debe validarse por SGR
        public bool validateCountry(string country, IConfiguration config)
        {
            bool response = true;
            List<string> lstPaises = new List<string>();
            lstPaises = new UtileriasAWM().getAppSettingsKey("CountrySGR", config).Split(',').ToList();
            try
            {
                foreach (var paisTemp in lstPaises)
                {
                    if (paisTemp == country)
                    {
                        response = false;
                    }
                }
                return response;
                //RequestSGR req = new RequestSGR();
                //ProxySGRt SGR = new ProxySGRt(req);
                //response = SGR.validateCountry(country);
            }
            catch (Exception)
            {
                return response;
            }
        }

        internal string mCalculoIVA(string oTramaRequest, int oPais, string oMoneda, string logkey, ref string oError, IConfiguration _Config, string workFlow, int workFlowID, string nodo)
        {
            DataSet oTabla = new DataSet();
            DataSet dCounty = new DataSet();
            string oRespuesta = "";
            double cargos = 0;
            double cargosmsg = 0;
            double gross = 0;
            double Iva = 0;
            double newGross = 0;
            double monto = 0;
            string oSP = "";
            double oCounty = 0;
            List<Parametros> iParams = new List<Parametros>();
            List<Parametros> iParams2 = new List<Parametros>();
            JObject miObject = JObject.Parse(oTramaRequest);
            //var nodo = encontrarValorNodo(miObject, "operation_id");

            try
            {
                //SaveLog(oTramaRequest, "1", logkey, 1, _Config);
                string oConnector = "";
                #region variables
                //string oMoneda = "";
                string elemento1 = "";
                if (workFlow == "SENDMONEYSTORE")
                {
                    var oDoc = ObtenerLocacion(_Config, workFlowID, ref miObject);
                    foreach (DataRow prop in oDoc.Rows)
                    {
                        string ValorNodo = prop["ValorNodoRequest"].ToString();
                        if (ValorNodo == "originators_principal_amount")
                        {
                            string NodoRuta = prop["NodoRuta"].ToString();
                            var elemento = NodoRuta.Replace("operationID", nodo);
                            cargos = Convert.ToDouble(elemento);
                            //elemento1 = elemento;
                        }
                        if (ValorNodo == "gross_total_amount")
                        {
                            string NodoRuta = prop["NodoRuta"].ToString();
                            var elemento = NodoRuta.Replace("operationID", nodo);
                            gross = Convert.ToDouble(elemento);
                            //elemento1 = elemento;
                        }
                        if (ValorNodo == "charges")
                        {
                            string NodoRuta = prop["NodoRuta"].ToString();
                            var elemento = NodoRuta.Replace("operationID", nodo);
                            monto = Convert.ToDouble(elemento);
                            //elemento1 = elemento;
                        }
                        if (ValorNodo == "message_charge")
                        {
                            string NodoRuta = prop["NodoRuta"].ToString();
                            var elemento = NodoRuta.Replace("operationID", nodo);
                            cargosmsg = Convert.ToDouble(elemento);
                            //elemento1 = elemento;
                        }
                        if (ValorNodo == "counter_id")
                        {
                            string NodoRuta = prop["NodoRuta"].ToString();
                            var elemento = NodoRuta.Replace("operationID", nodo);
                            oConnector = elemento;
                            //elemento1 = elemento;
                        }
                    }
                }
                if(workFlow == "FEEINQUIRY")
                {
                    cargos = 50.1 / 100;
                    gross = 200.75 / 100;
                    monto = 500.50 / 100;
                    cargosmsg = 5.7 / 100;
                    oConnector = getPropertyValue("fee-inquiry-request.foreign_remote_system.counter_id", miObject);
                    //var oDoc = ObtenerLocacion(_Config, workFlowID, ref miObject);
                    //foreach (DataRow prop in oDoc.Rows)
                    //{
                    //    string ValorNodo = prop["ValorNodo"].ToString();
                    //    if (ValorNodo == "originators_principal_amount")
                    //    {
                    //        string NodoRuta = prop["NodoRuta"].ToString();
                    //        var elemento = NodoRuta.Replace("operationID", nodo);
                    //        cargos = Convert.ToDouble(elemento);
                    //        //elemento1 = elemento;
                    //    }
                    //    if (ValorNodo == "gross_total_amount")
                    //    {
                    //        string NodoRuta = prop["NodoRuta"].ToString();
                    //        var elemento = NodoRuta.Replace("operationID", nodo);
                    //        gross = Convert.ToDouble(elemento);
                    //        //elemento1 = elemento;
                    //    }
                    //    if (ValorNodo == "charges")
                    //    {
                    //        string NodoRuta = prop["NodoRuta"].ToString();
                    //        var elemento = NodoRuta.Replace("operationID", nodo);
                    //        monto = Convert.ToDouble(elemento);
                    //        //elemento1 = elemento;
                    //    }
                    //    if (ValorNodo == "message_charge")
                    //    {
                    //        string NodoRuta = prop["NodoRuta"].ToString();
                    //        var elemento = NodoRuta.Replace("operationID", nodo);
                    //        cargosmsg = Convert.ToDouble(elemento);
                    //        //elemento1 = elemento;
                    //    }
                    //    if (ValorNodo == "counter_id")
                    //    {
                    //        string NodoRuta = prop["NodoRuta"].ToString();
                    //        var elemento = NodoRuta.Replace("operationID", nodo);
                    //        oConnector = elemento;
                    //        //elemento1 = elemento;
                    //    }
                    //}
                }
                #endregion
                string dbAlias = new UtileriasAWM().getAppSettingsKey("dbalias", _Config);
                MSSQLConnection objDB = new MSSQLConnection(dbAlias);
                #region Obtener County
                iParams.Add(new Parametros("@Pais", oPais, Parametros.SType.Int));
                iParams.Add(new Parametros("@Origen", monto, Parametros.SType.Decimal));
                iParams.Add(new Parametros("@Cargos", cargos, Parametros.SType.Decimal));
                iParams.Add(new Parametros("@Moneda", oMoneda, Parametros.SType.VarChar));
                iParams.Add(new Parametros("@CargosMSG", cargosmsg, Parametros.SType.Decimal));
                iParams.Add(new Parametros("@connectorid", oConnector, Parametros.SType.VarChar));
                Boolean oResult = false;
                try
                {
                    oResult = objDB.executeSP("remesadores.sp_GWMM_CalcularImpuesto", iParams, MSSQLConnection.ReturnTypes.Dataset);
                }
                catch (SqlException oExQ)
                {
                    oError = oExQ.Message;
                    objDB.closeConnection();
                    return oTramaRequest;
                }
                catch (Exception oEx)
                {
                    oError = oEx.Message;
                    objDB.closeConnection();
                    return oTramaRequest;
                }
                if (!oResult)
                {
                    oCounty = 0;
                }
                dCounty = objDB.getDataset();

                oCounty = Convert.ToDouble(dCounty.Tables[0].Rows[0]["Impuesto"].ToString());
                #endregion

                Iva = Math.Round(oCounty, 2);
                newGross = Math.Round((gross + Iva), 2);

                string sIva = "";
                string sNewGross = "";
                bool ob = mConvertirEnviado(Iva.ToString(), ref sIva, ref oError);
                bool ob2 = mConvertirEnviado(newGross.ToString(), ref sNewGross, ref oError);

                //if (workFlow == "SENDMONEY")
                //{
                //    JObject header = (JObject)miObject["send-money-store-request"]["financials"];
                //    header.Add("county_tax", sIva?.ToString() ?? "");
                //    header.Add("gross_total_amount", sNewGross?.ToString() ?? "");
                //    miObject["send-money-store-request"]["financials"] = header;
                //}
                //if (workFlow == "FEEINQUIRY")
                //{
                //    JObject header = (JObject)miObject["fee-inquiry-request"]["financials"];
                //    //var matches = header.Descendants().OfType<JObject>();
                //    //foreach (JObject jo in matches)
                //    //{
                //    //    header.Add("county_tax", sIva?.ToString() ?? "");
                //    //    header.Add("gross_total_amount", sNewGross?.ToString() ?? "");
                //    //}
                //    header.Add("county_tax", sIva?.ToString() ?? "");
                //    header.Add("gross_total_amount", sNewGross?.ToString() ?? "");
                //    miObject["fee-inquiry-request"]["financials"] = header;
                //}

                string countyTax = "";
                string grossTotal = "";

                if (workFlow == "SENDMONEYSTORE")
                {
                    var oDoc = ObtenerLocacion(_Config, workFlowID, ref miObject);
                    foreach (DataRow prop in oDoc.Rows)
                    {
                        string ValorNodo = prop["ValorNodoReplay"].ToString();
                        if (ValorNodo == "county_tax")
                        {
                            string NodoRuta = prop["NodoRuta"].ToString();
                            var elemento = NodoRuta.Replace("operationID", nodo);
                            countyTax = elemento;
                            //elemento1 = elemento;
                        }
                        if (ValorNodo == "gross_total_amount")
                        {
                            string NodoRuta = prop["NodoRuta"].ToString();
                            var elemento = NodoRuta.Replace("operationID", nodo);
                            grossTotal = elemento;
                            //elemento1 = elemento;
                        }
                    }
                    JObject header = (JObject)miObject[nodo]["financials"];//Ya viene en una variable
                    header.Add(countyTax, sIva?.ToString() ?? "");
                    header.Add(grossTotal, sNewGross?.ToString() ?? "");
                    miObject[nodo]["financials"] = header;
                }
                if (workFlow == "FEEINQUIRY")
                {
                    var oDoc = ObtenerLocacion(_Config, workFlowID, ref miObject);
                    foreach (DataRow prop in oDoc.Rows)
                    {
                        string ValorNodo = prop["ValorNodoReplay"].ToString();
                        if (ValorNodo == "county_tax")
                        {
                            string NodoRuta = prop["NodoRuta"].ToString();
                            var elemento = NodoRuta.Replace("operationID", nodo);
                            countyTax = elemento;
                            //elemento1 = elemento;
                        }
                        if (ValorNodo == "gross_total_amount")
                        {
                            string NodoRuta = prop["NodoRuta"].ToString();
                            var elemento = NodoRuta.Replace("operationID", nodo);
                            grossTotal = elemento;
                            //elemento1 = elemento;
                        }
                    }
                    JObject header = (JObject)miObject[nodo]["financials"];
                    header.Add(countyTax, sIva?.ToString() ?? "");
                    header.Add(grossTotal, sNewGross?.ToString() ?? "");
                    miObject[nodo]["financials"] = header;
                }
                oRespuesta = miObject.ToString();
            }
            catch (Exception oex)
            {
                oRespuesta = oError + " / Error: Calculo de iva.";
                //oRespuesta = oTrama;
            }

            return oRespuesta;
        }

        internal string GetStringElement(XElement element)
        {
            try
            {
                return element.Value.ToString();
            }
            catch (Exception)
            {
                return "";
            }
        }

        public bool mConvertirEnviado(string oTexto, ref string oCantidad, ref string oError)
        {
            bool oResultado = false;
            try
            {
                double pMonto = Convert.ToDouble(oTexto);
                double oEntero = Math.Truncate(pMonto);
                double oDecimal = pMonto - oEntero;
                oDecimal = (Math.Round(oDecimal, 2));

                if (oDecimal == 0)
                {
                    oCantidad = oEntero.ToString() + "00";
                }
                else
                {
                    if ((oDecimal > 0) && (oDecimal < 0.1))
                    {
                        oCantidad = (oEntero + oDecimal).ToString();
                    }
                    else
                    {
                        int oleng = oDecimal.ToString().Length;
                        switch (oleng)
                        {
                            case 3:
                                oCantidad = (oEntero + oDecimal).ToString() + "0";
                                break;
                            case 4:
                                oCantidad = (oEntero + oDecimal).ToString();
                                break;
                        }
                    }
                }
                oCantidad = oCantidad.ToString().Replace(",", "").Replace(".", "");
                oResultado = true;

            }
            catch (Exception oex)
            {
                oError = oex.Message;
                oResultado = false;
            }
            return oResultado;
        }

        internal dynamic consumeServicioValidaSGR(InfoSGR objlogateway, string uri, IConfiguration _config)
        {
            string json = string.Empty;
            try
            {
               
                var httpWebRequest = (HttpWebRequest)WebRequest.Create(uri);
                httpWebRequest.ContentType = "application/json";
                httpWebRequest.Method = "POST";
              
                using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                {
                    json = JsonConvert.SerializeObject(objlogateway);
                    streamWriter.Write(json);
                }

                var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                dynamic result = null;
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    result = streamReader.ReadToEnd();
                };

                return JObject.Parse(result);
            }
            catch (WebException ex)
            {
                var pageContent = new StreamReader(ex.Response.GetResponseStream()).ReadToEnd();
                var messageError = encontrarValorNodo(JObject.Parse(pageContent), "ExceptionMessage");
                throw new Exception(messageError);
            }
        }
    }
}