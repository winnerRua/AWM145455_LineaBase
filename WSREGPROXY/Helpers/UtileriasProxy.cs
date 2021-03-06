using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using WSREGPROXY.Encriptor;
using WSREGPROXY.Entities;
using WSREGPROXY.Services;
using WSSoapProxy.Models;

namespace WSREGPROXY.Helpers
{
    public class UtileriasProxy
    {
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
        internal string getAppSettingsKey(string KeyName, IConfiguration _config, string keySection)
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
        internal Dictionary<string, string> setRemesadorParameters(int remesadorID, string RemesadorShortName, IConfiguration config, string operationID, string countryCode, ref string errorMessage)
        {
            try
            {
                ConexionBDProxy oData = new ConexionBDProxy();
                var oTable = oData.getRemesadorConfig(remesadorID, RemesadorShortName, ref errorMessage, config);
                DataTable dtOperacionRemesador;
                Dictionary<string, string> auxObject = new Dictionary<string, string>();

                foreach (DataRow item in oTable.Rows)
                {
                    auxObject.Add("remesador_id", item["remesadorID"].ToString());
                    auxObject.Add("RemesadorEndpoint", item["RemesadorEndpoint"].ToString());
                    auxObject.Add("TipoServicio", item["TipoServicio"].ToString());
                    auxObject.Add("ServiceUser", item["ServiceUser"].ToString());
                    auxObject.Add("RemesadorServicePassword", item["RemesadorServicePassword"].ToString());
                    auxObject.Add("configremesadorID", item["configremesadorID"].ToString());
                    auxObject.Add("TipoRequestRemesador", item["TipoRequestRemesador"].ToString());
                    auxObject.Add("TieneNameSpace", item["TieneNameSpace"].ToString());
                    auxObject.Add("TipoCredencial", item["TipoCredencial"].ToString());
                    auxObject.Add("SecretKey", item["secretkey"].ToString());
                    auxObject.Add("Key", item["Key"].ToString());
                    auxObject.Add("operation_id", operationID);
                    auxObject.Add("country_code", countryCode);
                    auxObject.Add("CredencialUnicaRemesador", item["CredencialUnicaRemesador"].ToString());
                }
                dtOperacionRemesador = oData.getInterfazOperacionRemesador(Convert.ToInt32(auxObject["remesador_id"]), operationID, RemesadorShortName, ref errorMessage, config);
                foreach (DataRow item in dtOperacionRemesador.Rows)
                {
                    auxObject.Add("MetodoWSRemesador", item["MetodoWebService"].ToString());
                    auxObject.Add("ServiceName", item["ServiceName"].ToString());
                    auxObject.Add("TipoRespuesta", item["TipoRespuesta"].ToString());
                    auxObject.Add("TipoPeticion", item["TipoPeticion"].ToString());
                    auxObject.Add("NodoRespuesta", item["NodoRespuesta"].ToString());
                }
                return auxObject;
            }
            catch (Exception ex)
            {
                return new Dictionary<string, string>();
            }
        }
        internal void CreateJsonRequest(DataTable oTable, DataTable oTableSubItems, DataTable dtRemesadoresPartnersFields, Dictionary<string, string> infoGeneralRemesador, JObject tramaPartner, DataTable dtRemesadorDefaultValue, DataTable dtRemesadorCatalogos, ref JObject internalObject)
        {
            string partnerLocationError = string.Empty;
            string valorDefault = string.Empty;
            bool existeValorDefault = false;
            bool EvaluaSoloVacios = false;
            bool ExisteCatalogos = false;
            dynamic valorConvertido = null;

            try
            {
                foreach (DataRow subitem in oTableSubItems.Rows)
                {

                    if (subitem["TipoParametro"].ToString().ToUpper() == "OBJECT")
                    {
                        JObject auxJson = new JObject();
                        var oTableItems = ValidateDataTable(oTable, Convert.ToInt32(subitem["ParametroRemesadorID"].ToString()));
                        CreateJsonRequest(oTable, oTableItems, dtRemesadoresPartnersFields, infoGeneralRemesador, tramaPartner, dtRemesadorDefaultValue, dtRemesadorCatalogos, ref auxJson);
                        internalObject.Add(subitem["NombreParametro"].ToString(), auxJson);
                    }
                    else
                    {
                        var partnerFieldId = Convert.ToInt32(subitem["ParametroRemesadorID"]?.ToString() ?? "0");
                        var partnerLocation = dtRemesadoresPartnersFields.AsEnumerable().Where(x => x.Field<int>("ParametroRemesadorID") == partnerFieldId).Select(x => x.Field<string>("partnerlocation"))?.FirstOrDefault();
                        if (partnerLocation.Split('.')[0].ToUpper().Equals("OPERATIONID"))
                            partnerLocation = partnerLocation.Replace(partnerLocation.Split('.')[0], infoGeneralRemesador["operation_id"]);


                        var valorTramaPartner = new UtileriasProxy().getPropertyValue(partnerLocation, tramaPartner, subitem["FormatoFecha"].ToString());
                        ExisteCatalogos = dtRemesadorCatalogos.Rows.Count > 0 ? dtRemesadorCatalogos.AsEnumerable().Any(x => x.Field<int>("ParametroRemesadorID") == partnerFieldId) : false;
                        existeValorDefault = dtRemesadorDefaultValue.Rows.Count > 0 ? dtRemesadorDefaultValue.AsEnumerable().Any(x => x.Field<int>("ParametroRemesadorID") == partnerFieldId) : false;

                        if (valorTramaPartner != null)
                        {
                            if (valorTramaPartner != "")
                            {
                                valorConvertido = convertirValorTrama(subitem["TipoParametro"].ToString().ToUpper(), valorTramaPartner, subitem["FormatoFecha"].ToString());
                            }

                            // se evalua si el valor que se insertara en el nodo viene de un catalogo , valor default o de la trama del partner
                            if (ExisteCatalogos)
                            {
                                var valoresCatalogo = dtRemesadorCatalogos.AsEnumerable()
                                                         .Where(x => x.Field<int>("ParametroRemesadorID") == partnerFieldId)
                                                         .Where(x => x.Field<string>("ValorComparable").Contains(valorTramaPartner))
                                                         .Select(x => x.Field<string>("ValorRemesador"))?.FirstOrDefault();
                                if (!string.IsNullOrEmpty(valoresCatalogo))
                                    valorConvertido = convertirValorTrama(subitem["TipoParametro"].ToString().ToUpper(), valoresCatalogo, subitem["FormatoFecha"].ToString());
                                internalObject.Add(subitem["NombreParametro"].ToString(), valorConvertido);
                            }
                            //Se evalua si existe valores default
                            else if (existeValorDefault)
                            {
                                EvaluaSoloVacios = (bool)dtRemesadorDefaultValue.AsEnumerable().Where(x => x.Field<int>("ParametroRemesadorID") == partnerFieldId).Select(x => x.Field<bool>("EvaluaSoloNulos"))?.FirstOrDefault();
                                valorDefault = dtRemesadorDefaultValue.AsEnumerable().Where(x => x.Field<int>("ParametroRemesadorID") == partnerFieldId).Select(x => x.Field<string>("valor"))?.FirstOrDefault();
                                if (valorTramaPartner == "" && EvaluaSoloVacios)
                                {
                                    if (!string.IsNullOrEmpty(valorDefault))
                                    {
                                        valorConvertido = convertirValorTrama(subitem["TipoParametro"].ToString().ToUpper(), valorDefault, subitem["FormatoFecha"].ToString());
                                        internalObject.Add(subitem["NombreParametro"].ToString(), valorConvertido);
                                    }
                                }
                                else
                                {
                                    if (!string.IsNullOrEmpty(valorDefault))
                                    {
                                        valorConvertido = convertirValorTrama(subitem["TipoParametro"].ToString().ToUpper(), valorDefault, subitem["FormatoFecha"].ToString());
                                        internalObject.Add(subitem["NombreParametro"].ToString(), valorConvertido);
                                    }
                                }
                            }
                            //si no existe se ingresa norrmal
                            else
                            {
                                internalObject.Add(subitem["NombreParametro"].ToString(), valorConvertido);
                            }
                        }
                        else
                        {
                            if (existeValorDefault && subitem["EsRequerido"].ToString() == "True")
                            {
                                valorDefault = dtRemesadorDefaultValue.AsEnumerable().Where(x => x.Field<int>("ParametroRemesadorID") == partnerFieldId).Select(x => x.Field<string>("valor"))?.FirstOrDefault();
                                valorConvertido = convertirValorTrama(subitem["TipoParametro"].ToString().ToUpper(), valorDefault, subitem["FormatoFecha"].ToString());
                                internalObject.Add(subitem["NombreParametro"].ToString(), valorConvertido);
                            }
                            else
                            {
                                partnerLocationError += partnerLocation + " | ";
                            }
                        }
                    }
                }
                if (!partnerLocationError.Equals(string.Empty))
                {
                    throw new Exception("Las siguientes rutas del partner estan mal configuradas: " + partnerLocationError);
                }
            }
            catch (Exception ex) { throw ex; }
        }
        internal dynamic convertirValorTrama(string type, dynamic valorTramaPartner, string formatoFecha)
        {

            dynamic valorConvertido = null;
            try
            {
                if (type == "STRING")
                {
                    valorConvertido = Convert.ToString(valorTramaPartner);
                }
                else if (type == "INT")
                {
                    valorConvertido = Convert.ToInt32(valorTramaPartner);
                }

                else if (type == "DOUBLE")
                {
                    valorConvertido = Convert.ToDouble(valorTramaPartner);
                }

                else if (type == "BOOL")
                {
                    valorConvertido = Convert.ToBoolean(valorTramaPartner);
                }

                else if (type == "DATETIME")
                {
                    DateTime fecha = Convert.ToDateTime(valorTramaPartner);
                    valorConvertido = fecha.ToString(formatoFecha, CultureInfo.InvariantCulture);
                }
                return valorConvertido;
            }
            catch (Exception ex) { throw ex; }
        }
        internal void CreateGetRequest(DataTable oTable, DataTable oTableSubItems, DataTable dtRemesadoresPartnersFields, Dictionary<string, string> infoGeneralRemesador, JObject tramaPartner, ref JObject internalObject)
        {
            //string partnerLocationError = string.Empty;
            //try
            //{
            //    foreach (DataRow subitem in oTableSubItems.Rows)
            //    {
            //        switch (subitem["TipoParametro"].ToString().ToUpper())
            //        {
            //            case "OBJECT":
            //                JObject auxJson = new JObject();
            //                var oTableItems = ValidateDataTable(oTable, Convert.ToInt32(subitem["ParametroRemesadorID"].ToString()));
            //                CreateJsonRequest(oTable, oTableItems, dtRemesadoresPartnersFields, infoGeneralRemesador, tramaPartner, ref auxJson);
            //                internalObject.Add(subitem["NombreParametro"].ToString(), auxJson);
            //                break;
            //            case "STRING":

            //                var partnerFieldId = Convert.ToInt32(subitem["ParametroRemesadorID"]?.ToString() ?? "0");
            //                var partnerLocation = dtRemesadoresPartnersFields.AsEnumerable().Where(x => x.Field<int>("ParametroRemesadorID") == partnerFieldId).Select(x => x.Field<string>("partnerlocation"))?.FirstOrDefault();

            //                if (partnerLocation.Split('.')[0].ToUpper().Equals("OPERATIONID"))
            //                    partnerLocation = partnerLocation.Replace(partnerLocation.Split('.')[0], infoGeneralRemesador["operation_id"]);

            //                var valorTramaPartner = new UtileriasProxy().getPropertyValue(partnerLocation, tramaPartner);
            //                if (valorTramaPartner == null)
            //                {
            //                    partnerLocationError += partnerLocation + " | ";

            //                }
            //                else
            //                {
            //                    internalObject.Add(subitem["NombreParametro"].ToString(), valorTramaPartner);
            //                }
            //                break;
            //        }
            //    }
            //    if (!partnerLocationError.Equals(string.Empty))
            //    {
            //        throw new Exception("Las siguientes rutas del partner estan mal configuradas: " + partnerLocationError);
            //    }
            //}
            //catch (Exception ex) { }
        }
        internal void CreateXmlRequest(DataTable oTable, DataTable oTableSubItems, DataTable dtRemesadoresPartnersFields, Dictionary<string, string> infoGeneralRemesador, JObject tramaPartner, DataTable dtRemesadorCatalogos, DataTable dtRemesadorDefaultValue, ref XElement internalElement)
        {
            string partnerLocationError = string.Empty;
            string valorDefault = string.Empty;
            bool existeValorDefault = false;
            bool EvaluaSoloVacios = false;
            bool ExisteCatalogos = false;
            dynamic valorConvertido = null;
            try
            {
                foreach (DataRow subitem in oTableSubItems.Rows)
                {
                    if (subitem["TipoParametro"].ToString().ToUpper() == "OBJECT")
                    {

                        XElement auxElement = new XElement(subitem["NombreParametro"].ToString());
                        var oTableItems = ValidateDataTable(oTable, Convert.ToInt32(subitem["ParametroRemesadorID"].ToString()));
                        CreateXmlRequest(oTable, oTableItems, dtRemesadoresPartnersFields, infoGeneralRemesador, tramaPartner, dtRemesadorCatalogos, dtRemesadorDefaultValue, ref auxElement);
                        internalElement.Add(auxElement);

                    }
                    else
                    {
                        var partnerFieldId = Convert.ToInt32(subitem["ParametroRemesadorID"]?.ToString() ?? "0");
                        var partnerLocation = dtRemesadoresPartnersFields.AsEnumerable().Where(x => x.Field<int>("ParametroRemesadorID") == partnerFieldId).Select(x => x.Field<string>("partnerlocation"))?.FirstOrDefault();

                        if (partnerLocation.Split('.')[0].ToUpper().Equals("OPERATIONID"))
                            partnerLocation = partnerLocation.Replace(partnerLocation.Split('.')[0], infoGeneralRemesador["operation_id"]);

                        var valorTramaPartner = new UtileriasProxy().getPropertyValue(partnerLocation, tramaPartner , subitem["FormatoFecha"].ToString());

                        ExisteCatalogos = dtRemesadorCatalogos.Rows.Count > 0 ? dtRemesadorCatalogos.AsEnumerable().Any(x => x.Field<int>("ParametroRemesadorID") == partnerFieldId) : false;
                        existeValorDefault = dtRemesadorDefaultValue.Rows.Count > 0 ? dtRemesadorDefaultValue.AsEnumerable().Any(x => x.Field<int>("ParametroRemesadorID") == partnerFieldId) : false;

                        if (valorTramaPartner != null)
                        {
                            if (valorTramaPartner != "")
                            {
                                valorConvertido = convertirValorTrama(subitem["TipoParametro"].ToString().ToUpper(), valorTramaPartner, subitem["FormatoFecha"].ToString());
                            }
                          
                            // se evalua si el valor que se insertara en el nodo viene de un catalogo , valor default o de la trama del partner
                            if (ExisteCatalogos)
                            {
                                var valoresCatalogo = dtRemesadorCatalogos.AsEnumerable()
                                                         .Where(x => x.Field<int>("ParametroRemesadorID") == partnerFieldId)
                                                         .Where(x => x.Field<string>("ValorComparable").Contains(valorTramaPartner))
                                                         .Select(x => x.Field<string>("ValorRemesador"))?.FirstOrDefault();
                                if (!string.IsNullOrEmpty(valoresCatalogo))
                                {
                                    valorConvertido = convertirValorTrama(subitem["TipoParametro"].ToString().ToUpper(), valoresCatalogo, subitem["FormatoFecha"].ToString());
                                    internalElement.Add(new XElement(subitem["NombreParametro"].ToString(), valorConvertido));
                                }
                            }
                            else if (existeValorDefault)
                            {
                                EvaluaSoloVacios = (bool)dtRemesadorDefaultValue.AsEnumerable().Where(x => x.Field<int>("ParametroRemesadorID") == partnerFieldId).Select(x => x.Field<bool>("EvaluaSoloNulos"))?.FirstOrDefault();
                                valorDefault = dtRemesadorDefaultValue.AsEnumerable().Where(x => x.Field<int>("ParametroRemesadorID") == partnerFieldId).Select(x => x.Field<string>("valor"))?.FirstOrDefault();
                                if (valorTramaPartner == "" && EvaluaSoloVacios)
                                {
                                    if (!string.IsNullOrEmpty(valorDefault))
                                    {
                                        valorConvertido = convertirValorTrama(subitem["TipoParametro"].ToString().ToUpper(), valorDefault, subitem["FormatoFecha"].ToString());
                                        internalElement.Add(new XElement(subitem["NombreParametro"].ToString(), valorConvertido));
                                    }
                                }
                                else
                                {
                                    if (!string.IsNullOrEmpty(valorDefault))
                                    {
                                        valorConvertido = convertirValorTrama(subitem["TipoParametro"].ToString().ToUpper(), valorDefault, subitem["FormatoFecha"].ToString());
                                        internalElement.Add(new XElement(subitem["NombreParametro"].ToString(), valorConvertido));
                                    }
                                }
                            }
                            else
                            {
                                internalElement.Add(new XElement(subitem["NombreParametro"].ToString(), valorConvertido));
                            }
                        }
                        else
                        {

                            if (existeValorDefault && subitem["EsRequerido"].ToString() == "True")
                            {
                                valorDefault = dtRemesadorDefaultValue.AsEnumerable().Where(x => x.Field<int>("ParametroRemesadorID") == partnerFieldId).Select(x => x.Field<string>("valor"))?.FirstOrDefault();
                                valorConvertido = convertirValorTrama(subitem["TipoParametro"].ToString().ToUpper(), valorDefault, subitem["FormatoFecha"].ToString());
                                internalElement.Add(new XElement(subitem["NombreParametro"].ToString(), valorConvertido));
                            }
                            else
                            {
                                partnerLocationError += partnerLocation + " | ";
                            }
                        }
                    }
                }
                if (!partnerLocationError.Equals(string.Empty))
                {

                    throw new Exception("Las siguientes rutas del partner estan mal configuradas: " + partnerLocationError);
                }
            }
            catch (Exception ex) { throw ex; }
        }
        internal void CreateXmlRequest(DataTable NameSpaceParam, Dictionary<int, XNamespace> LstNameSpace, Dictionary<int, string> LstPrefix, int RemesadorParamID, string ParamName, DataTable oTable, DataTable oTableSubItems, DataTable dtRemesadoresPartnersFields, Dictionary<string, string> infoGeneralRemesador, JObject tramaPartner, DataTable dtRemesadorCatalogos, DataTable dtRemesadorDefaultValue, ref XElement internalElement)
        {
            string partnerLocationError = string.Empty;
            string valorDefault = string.Empty;
            bool existeValorDefault = false;
            bool EvaluaSoloVacios = false;
            bool ExisteCatalogos = false;
            dynamic valorConvertido = null;
            try
            {

                foreach (DataRow subitem in oTableSubItems.Rows)
                {


                    if (subitem["TipoParametro"].ToString().ToUpper() == "OBJECT")
                    {
                        XElement auxElement = setXElement(NameSpaceParam, LstNameSpace, LstPrefix, Convert.ToInt32(subitem["ParametroRemesadorID"].ToString()), subitem["NombreParametro"].ToString());
                        var oTableItems = ValidateDataTable(oTable, Convert.ToInt32(subitem["ParametroRemesadorID"].ToString()));
                        CreateXmlRequest(NameSpaceParam, LstNameSpace, LstPrefix, Convert.ToInt32(subitem["ParametroRemesadorID"].ToString()), subitem["NombreParametro"].ToString(), oTable, oTableItems, dtRemesadoresPartnersFields, infoGeneralRemesador, tramaPartner, dtRemesadorCatalogos, dtRemesadorDefaultValue, ref auxElement);
                        internalElement.Add(auxElement);

                    }
                    else
                    {
                        XElement stringElement = setXElement(NameSpaceParam, LstNameSpace, LstPrefix, Convert.ToInt32(subitem["ParametroRemesadorID"].ToString()), subitem["NombreParametro"].ToString());

                        var partnerFieldId = Convert.ToInt32(subitem["ParametroRemesadorID"]?.ToString() ?? "0");
                        var partnerLocation = dtRemesadoresPartnersFields.AsEnumerable().Where(x => x.Field<int>("ParametroRemesadorID") == partnerFieldId).Select(x => x.Field<string>("partnerlocation"))?.FirstOrDefault();

                        if (partnerLocation.Split('.')[0].ToUpper().Equals("OPERATIONID"))
                            partnerLocation = partnerLocation.Replace(partnerLocation.Split('.')[0], infoGeneralRemesador["operation_id"]);
                        var valorTramaPartner = new UtileriasProxy().getPropertyValue(partnerLocation, tramaPartner, subitem["FormatoFecha"].ToString());

                        //se obtienen los valores default configurados


                        ExisteCatalogos = dtRemesadorCatalogos.Rows.Count > 0 ? dtRemesadorCatalogos.AsEnumerable().Any(x => x.Field<int>("ParametroRemesadorID") == partnerFieldId) : false;
                        existeValorDefault = dtRemesadorDefaultValue.Rows.Count > 0 ? dtRemesadorDefaultValue.AsEnumerable().Any(x => x.Field<int>("ParametroRemesadorID") == partnerFieldId) : false;



                        if (dtRemesadorDefaultValue.Rows.Count > 0)
                        {
                            existeValorDefault = dtRemesadorDefaultValue.AsEnumerable().Any(x => x.Field<int>("ParametroRemesadorID") == partnerFieldId);
                        }
                        if (valorTramaPartner != null)
                        {

                            if (valorTramaPartner != "")
                            {

                                valorConvertido = convertirValorTrama(subitem["TipoParametro"].ToString().ToUpper(), valorTramaPartner, subitem["FormatoFecha"].ToString());

                            }
                            if (dtRemesadorCatalogos.Rows.Count > 0)
                            {
                                ExisteCatalogos = dtRemesadorCatalogos.AsEnumerable().Any(x => x.Field<int>("ParametroRemesadorID") == partnerFieldId);
                            }
                            // se evalua si el valor que se insertara en el nodo viene de un catalogo , valor default o de la trama del partner
                            if (ExisteCatalogos)
                            {
                                var valoresCatalogo = dtRemesadorCatalogos.AsEnumerable()
                                                         .Where(x => x.Field<int>("ParametroRemesadorID") == partnerFieldId)
                                                         .Where(x => x.Field<string>("ValorComparable").Contains(valorTramaPartner))
                                                         .Select(x => x.Field<string>("ValorRemesador"))?.FirstOrDefault();

                                if (!string.IsNullOrEmpty(valoresCatalogo))
                                {
                                    valorConvertido = convertirValorTrama(subitem["TipoParametro"].ToString().ToUpper(), valoresCatalogo, subitem["FormatoFecha"].ToString());

                                    stringElement.Value = valorConvertido;
                                    internalElement.Add(stringElement);
                                }
                            }

                            else if (existeValorDefault)
                            {
                                EvaluaSoloVacios = (bool)dtRemesadorDefaultValue.AsEnumerable().Where(x => x.Field<int>("ParametroRemesadorID") == partnerFieldId).Select(x => x.Field<bool>("EvaluaSoloNulos"))?.FirstOrDefault();
                                valorDefault = dtRemesadorDefaultValue.AsEnumerable().Where(x => x.Field<int>("ParametroRemesadorID") == partnerFieldId).Select(x => x.Field<string>("valor"))?.FirstOrDefault();
                                if (valorTramaPartner == "" && EvaluaSoloVacios)
                                {
                                    if (!string.IsNullOrEmpty(valorDefault))
                                    {
                                        valorConvertido = convertirValorTrama(subitem["TipoParametro"].ToString().ToUpper(), valorDefault, subitem["FormatoFecha"].ToString());

                                        stringElement.SetValue(valorConvertido);
                                        internalElement.Add(stringElement);
                                    }
                                }
                                else
                                {
                                    if (!string.IsNullOrEmpty(valorDefault))
                                    {
                                        valorConvertido = convertirValorTrama(subitem["TipoParametro"].ToString().ToUpper(), valorDefault, subitem["FormatoFecha"].ToString());

                                        stringElement.SetValue(valorConvertido);
                                        internalElement.Add(stringElement);
                                    }
                                }
                            }
                            else
                            {
                                if (!string.IsNullOrEmpty(valorTramaPartner))
                                {
                                    stringElement.SetValue(valorConvertido);
                                    internalElement.Add(stringElement);
                                }
                            }
                        }
                        else
                        {
                            if (existeValorDefault && subitem["EsRequerido"].ToString() == "True")
                            {
                                valorDefault = dtRemesadorDefaultValue.AsEnumerable().Where(x => x.Field<int>("ParametroRemesadorID") == partnerFieldId).Select(x => x.Field<string>("valor"))?.FirstOrDefault();
                                valorConvertido = convertirValorTrama(subitem["TipoParametro"].ToString().ToUpper(), valorDefault, subitem["FormatoFecha"].ToString());
                                stringElement.SetValue(valorConvertido);

                                //if (valorTramaPartner != null)
                                //                             {
                                //                               switch (valorTramaPartner.Type)
                                //                             {
                                //                               case JTokenType.Integer:
                                //                                 stringElement.Value = Convert.ToInt32(valorTramaPartner);
                                //                               break;
                                //                         case JTokenType.Float:
                                //                           stringElement.Value = Convert.ToDouble(valorTramaPartner);
                                //                         break;
                                //                   case JTokenType.Date:
                                //                     stringElement.Value = valorTramaPartner.ToString("yyyy-MM-ddThh:mm:ss");
                                //                   break;
                                //             default:
                                //               stringElement.Value = valorTramaPartner;
                                //             break;
                                //}
                                //}
                                internalElement.Add(stringElement);

                            }
                            else
                            {
                                partnerLocationError += partnerLocation + " | ";
                            }
                        }
                    }
                }


                if (!partnerLocationError.Equals(string.Empty))
                {
                    throw new Exception("Las siguientes rutas del partner estan mal configuradas: " + partnerLocationError);
                }
            }
            catch (Exception ex) { throw ex; }
        }
        internal DataTable ValidateDataTable(DataTable oTable, int ParamID)
        {
            DataTable oTableItems = new DataTable();
            var data = oTable.AsEnumerable().Where(x => x.Field<int?>("ParametroPadre") != null && x.Field<int?>("ParametroPadre") == ParamID);

            if (data.Count() > 0)
                oTableItems = oTable.AsEnumerable().Where(x => x.Field<int?>("ParametroPadre") != null && x.Field<int?>("ParametroPadre") == ParamID).CopyToDataTable();
            else
                oTableItems = new DataTable();

            return oTableItems;

        }
        internal XElement setXElement(DataTable NameSpaceParam, Dictionary<int, XNamespace> LstNameSpace, Dictionary<int, string> LstPrefix, int RemesadorParamID, string ParamName)
        {

            XElement oDoc_Rest = null;
            if (NameSpaceParam.AsEnumerable().Where(x => x.Field<int>("ParametroRemesadorID") == RemesadorParamID).Count() == 0)
                return new XElement(ParamName);

            dynamic ParamTable = NameSpaceParam.AsEnumerable().Where(x => x.Field<int>("ParametroRemesadorID") == RemesadorParamID).CopyToDataTable();
            try
            {
                if (ParamTable.Rows.Count > 0)
                {
                    var intNameSpace = NameSpaceParam.AsEnumerable().Where(x => x.Field<bool>("UsarComoPrefijo") == true && x.Field<int>("ParametroRemesadorID") == RemesadorParamID).Select(x => x.Field<int>("NameSpaceID")).FirstOrDefault();
                    var auxNameSpace = LstNameSpace.Where(x => x.Key == intNameSpace).Select(x => x.Value).FirstOrDefault();

                    if (auxNameSpace != null)
                    {
                        oDoc_Rest = new XElement(auxNameSpace + ParamName);
                    }
                    else
                    {
                        oDoc_Rest = new XElement(ParamName);
                    }
                    DataTable auxAttributesTable = new DataTable();
                    var data = NameSpaceParam.AsEnumerable().Where(x => x.Field<bool>("UsarComoAtributo") == true && x.Field<int>("ParametroRemesadorID") == RemesadorParamID);

                    if (data.Count() > 0)
                        auxAttributesTable = NameSpaceParam.AsEnumerable().Where(x => x.Field<bool>("UsarComoAtributo") == true && x.Field<int>("ParametroRemesadorID") == RemesadorParamID).CopyToDataTable();
                    else
                        auxAttributesTable = new DataTable();

                    foreach (DataRow attributes in auxAttributesTable.Rows)
                    {
                        if (!string.IsNullOrEmpty(LstPrefix.Where(x => x.Key == Convert.ToInt32(attributes["NameSpaceID"].ToString())).Select(x => x.Value).FirstOrDefault()))
                            oDoc_Rest.Add(new XAttribute(XNamespace.Xmlns + LstPrefix.Where(x => x.Key == Convert.ToInt32(attributes["NameSpaceID"].ToString())).Select(x => x.Value).FirstOrDefault(), LstNameSpace.Where(x => x.Key == Convert.ToInt32(attributes["NameSpaceID"].ToString())).Select(x => x.Value).FirstOrDefault()));
                        else
                        {
                            oDoc_Rest = null;
                            oDoc_Rest = new XElement(LstNameSpace.Where(x => x.Key == Convert.ToInt32(attributes["NameSpaceID"].ToString())).Select(x => x.Value).FirstOrDefault() + ParamName);
                        }
                    }
                }
                else
                {
                    oDoc_Rest = new XElement(ParamName);
                }
            }
            catch (Exception ex) { throw ex; }

            return oDoc_Rest;
        }
        internal string GenerarJWT(IConfiguration _config, Dictionary<string, string> remesadorInfo, DataTable valoresJWT)
        {
            try
            {
                string oError = string.Empty;
                DataTable InfoHeaders = new DataTable();
                var mySecurityKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(remesadorInfo["SecretKey"]));
                string myIssuer = string.Empty;
                string myAudience = string.Empty;
                foreach (DataRow item in valoresJWT.Rows)
                {
                    if (item["property"].ToString() == "Issuer")
                    {
                        myIssuer = item["property"].ToString();
                    }
                    if (item["property"].ToString() == "Audience")
                    {

                        myAudience = item["property"].ToString();
                    }
                }
                var tokenHandler = new JwtSecurityTokenHandler();
                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Expires = DateTime.MaxValue,
                    Issuer = myIssuer,
                    Audience = myAudience,
                    SigningCredentials = new SigningCredentials(mySecurityKey, SecurityAlgorithms.HmacSha256Signature)
                };
                var token = tokenHandler.CreateToken(tokenDescriptor);
                return tokenHandler.WriteToken(token);
            }
            catch (Exception ex)
            {

                throw ex;
            }

        }
        internal X509Certificate GetCertData(int remesadorID, IConfiguration _config)
        {
            try
            {
                string oError = string.Empty;
                string friendlyName = string.Empty;
                DataTable dtInfoCertificado;
                //dtInfoCertificado = new ConexionBDProxy().getRemesadorCertificadoInfo(Convert.ToInt32(remesadorID), ref oError, _config);

                //foreach (DataRow item in dtInfoCertificado.Rows)
                //{
                //    friendlyName = item["FriendlyName"].ToString();
                //}

                //BFEngine encriptor = new BFEngine(GetInternalPassKey());

                //string CertName = encriptor.Process(friendlyName, BFEngine.AccionCifrado.Decrypt);

                ////return new X509Certificate(CertName, CertPass);
                return FindCertificate(StoreLocation.LocalMachine, StoreName.My, X509FindType.FindBySubjectName, friendlyName);


            }
            catch (Exception ex)
            {
                try
                {

                    BFEngine encriptor = new BFEngine();
                    String CertName = encriptor.Process("", BFEngine.AccionCifrado.Decrypt);
                    return FindCertificate(StoreLocation.LocalMachine, StoreName.My,
                                                              X509FindType.FindBySubjectName,
                                                              CertName);
                }
                catch
                {
                    throw ex;
                }
            }

        }
        static X509Certificate2 FindCertificate(StoreLocation location, StoreName name, X509FindType findType, string findValue)
        {
            X509Store store = new X509Store(name, location);
            try
            {
                // create and open store for read-only access
                store.Open(OpenFlags.ReadOnly);

                // search store
                X509Certificate2Collection col = store.Certificates;
                foreach (X509Certificate2 icert in col)
                {
                    if (icert.FriendlyName.ToString().CompareTo(findValue) == 0)
                    {
                        return icert;
                    }

                }
                throw new Exception(String.Format("No existe el certificado {0}, {1}, {2}, {3}", findValue, findType.ToString(), location.ToString(), name.ToString()));

                // return first certificate found
                //return col[0];
            }
            catch (Exception)
            {
                throw new Exception(String.Format("No existe el certificado {0}, {1}, {2}, {3}", findValue, findType.ToString(), location.ToString(), name.ToString()));
            }
            // always close the store
            finally { store.Close(); }
        }
        internal static string GetInternalPassKey()
        {
            return "gRup0_c03N_h2H";
        }
        public dynamic getPropertyValue(string compoundProperty, JObject myObject, string formatoFecha = "")
        {
            string[] bits = compoundProperty.Split('.');

            for (int i = 0; i < bits.Length - 1; i++)
            {
                var auxObject = myObject.Properties().Where(prop => prop.Name?.ToUpper() == bits[i].ToUpper()).FirstOrDefault()?.Value?.ToString();
                if (!string.IsNullOrEmpty(auxObject))
                    myObject = JObject.Parse(auxObject);

            }

            var jToken = myObject.Properties().Where(prop => prop.Name.ToLower() == bits[bits.Length - 1].ToLower()).FirstOrDefault()?.Value;

            if (jToken != null)
            {
                if (jToken.Type == JTokenType.Date && formatoFecha != string.Empty)
                {
                    DateTime stringReq = Convert.ToDateTime(jToken.ToString());
                    jToken = stringReq.ToString(formatoFecha);
                }
                return jToken.ToString();
            }
            else
            {
                return null;
            }
        }
        internal dynamic consumeServicioRest(dynamic trama, Dictionary<string, string> infoGeneralRemesador, MyWebClient client, Dictionary<string, string> paramGetRest = null)
        {
            try
            {
                JObject resputaGWMM = new JObject();
                XmlDocument respuestaXml = new XmlDocument();
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11;
                byte[] postData = Encoding.UTF8.GetBytes(trama.ToString());
                client.BaseAddress = infoGeneralRemesador["RemesadorEndpoint"];
                if (paramGetRest != null)
                {
                    foreach (var item in paramGetRest)
                    {
                        client.QueryString.Add(item.Key, item.Value);
                    }
                }
                byte[] response = client.UploadData(infoGeneralRemesador["RemesadorEndpoint"], infoGeneralRemesador["TipoPeticion"], postData);
                if (response.Length == 0)
                {
                    throw new Exception("No hubo respuesta del remesador.");
                }
                if (infoGeneralRemesador["TipoRespuesta"].ToUpper() == "XML")
                {
                    respuestaXml = new XmlDocument();
                    respuestaXml.LoadXml(Encoding.UTF8.GetString(response));
                    var miJObject = JObject.Parse(JsonConvert.SerializeXmlNode(respuestaXml));
                    return JObject.Parse(encontrarValorNodo(miJObject, infoGeneralRemesador["NodoRespuesta"]));
                }
                return JObject.Parse(Encoding.UTF8.GetString(response));
            }
            catch (WebException ex)
            {
                WebResponse errResp = ex.Response;
                string text = string.Empty;
                using (Stream respStream = errResp.GetResponseStream())
                {
                    StreamReader reader = new StreamReader(respStream);
                    text = reader.ReadToEnd();
                }
                throw new Exception(text);
            }
        }
        internal dynamic consumeServicioSoap(dynamic trama, string keylog, IConfiguration _config)
        {
            string json = string.Empty;
            string uri = string.Empty;
            try
            {
                uri = new UtileriasProxy().getAppSettingsKey("servicioConsumeWS", _config, "Servicios");
                var httpWebRequest = (HttpWebRequest)WebRequest.Create(uri);
                httpWebRequest.ContentType = "application/json";
                httpWebRequest.Method = "POST";
                InfoProxy objlogateway = new InfoProxy()
                {
                    tramaProxy = Convert.ToString(trama),
                    keyLog = keylog
                };
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
        internal string GenerarJWTLog(IConfiguration _config)
        {
            try
            {
                var mySecretKey = string.Empty;
                var stringError = string.Empty;
                mySecretKey = getAppSettingsKey("SecretKey", _config, "AppSettings");
                var mySecurityKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(mySecretKey));
                var myIssuer = "gateway";
                var myAudience = "http://localhost:5001";
                var tokenHandler = new JwtSecurityTokenHandler();
                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Expires = DateTime.MaxValue,
                    Issuer = myIssuer,
                    Audience = myAudience,
                    SigningCredentials = new SigningCredentials(mySecurityKey, SecurityAlgorithms.HmacSha256Signature)
                };
                var token = tokenHandler.CreateToken(tokenDescriptor);
                return tokenHandler.WriteToken(token);
            }
            catch (Exception ex)
            {

                throw ex;
            }

        }
        public void SaveLog(dynamic pMetadata, string identificadorEvento, string keyEvent, int ComponentID, IConfiguration _config)
        {
            string oError = "";
            try
            {
                oError = string.Empty;
                SaveLogDataDelegate.SaveLogDataDelegateAsync(pMetadata, identificadorEvento, keyEvent, ComponentID, _config);
            }
            catch (Exception ex)
            {
                oError = ex.Message;
            }
        }
        public JObject BuildResponse(ACGError error, string descripcion, string agency_code, string sub_agent_user)
        {
            int oerror = (int)error;
            string oError = "";
            JObject miObjectResponse = new JObject();
            JObject miFault = new JObject();
            JObject miDetail = new JObject();
            JObject mierror = new JObject();
            if (oerror < 100)
            {
                oError = "0" + oerror.ToString();
            }
            if (agency_code == string.Empty && sub_agent_user == string.Empty)
            {
                miObjectResponse.Add("Fault", miFault);
                miFault.Add("faultcode", "AIRPAK-ERROR");
                miFault.Add("faultstring", descripcion);
                miFault.Add("faultactor", "n/a - n/a");
                miDetail.Add("detail", mierror);
                mierror.Add("error", "AIR" + oError + " " + error.ToString());
                mierror.Add("ExternalReferenceNumber", "");
                mierror.Add("PartnerID", "");
                return miObjectResponse;
            }
            else
            {
                miObjectResponse.Add("Fault", miFault);
                miFault.Add("faultcode", "AIRPAK-ERROR");
                miFault.Add("faultstring", descripcion);
                miFault.Add("faultactor", sub_agent_user + " -  " + agency_code);
                miFault.Add("detail", miDetail);
                miDetail.Add("airpak_error", mierror);
                mierror.Add("error", "AIR" + oError + " " + error.ToString());
                mierror.Add("ExternalReferenceNumber", "");
                mierror.Add("PartnerID", "");
                return miObjectResponse;
            }
        }

    }
}
