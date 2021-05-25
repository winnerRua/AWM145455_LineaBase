using GrupoCoen.Corporativo.Libraries.AH2HAS.Utilidades;
using GrupoCoen.Corporativo.ProjectCenter.Libraries.CriptoEngine;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Xml.Linq;
using WSREGPROXY.Helpers;
using WSREGPROXY.Services;
using WSSoapProxy.Models;

namespace WSREGPROXY
{
    public class Proxy
    {
        MyWebClient headers = new MyWebClient();
        string keylog = string.Empty;
        string tramaDelPartner = string.Empty;
        string agency_code = string.Empty;
        string sub_agent_user = string.Empty;
        public dynamic ConsumeProxy(string tramaPartner, string keyLog, IConfiguration _config)
        {

            #region Variables
            tramaDelPartner = tramaPartner;
            this.keylog = keyLog;
            ConexionBDProxy miConexionBD = new ConexionBDProxy();
            DataTable dtHeaders = new DataTable();
            DataTable dtRemesadorInfo = new DataTable();
            Dictionary<string, string> infoGeneralRemesador = new Dictionary<string, string>();
            Dictionary<string, string> paramGetRest = null;
            UtileriasProxy miUtilerias = new UtileriasProxy();
            JObject JsonPartner;
            AS_Generales oGeneral = new AS_Generales();
            dynamic tramaRemesador = string.Empty;
            string oError = string.Empty, respuesta = string.Empty, requestRemesador = string.Empty;
            string remesadorShortName = string.Empty, countryCode = string.Empty, operationID = string.Empty;
            dynamic responseRemesador;
            int remesadorID = 0;
            #endregion
            try
            {

                oError = string.Empty;
                JsonPartner = JObject.Parse(tramaPartner);
                remesadorShortName = miUtilerias.getPropertyValue("Header.operation_product_id", JsonPartner);
                operationID = miUtilerias.getPropertyValue("Header.operation_id", JsonPartner);
                countryCode = miUtilerias.getPropertyValue("Header.country_code", JsonPartner);
                agency_code = miUtilerias.getPropertyValue("Header.agency_code", JsonPartner);
                remesadorID = Convert.ToInt32(miUtilerias.getPropertyValue("Header.remesador_id", JsonPartner) ?? "0");

                if (remesadorID == 0)
                {
                    var responseError = miUtilerias.BuildResponse(ACGError.RequestIsNullOrEmpty, "No se pudo obtener identificador de remesador", agency_code, sub_agent_user);
                    miUtilerias.SaveLog(responseError, "1", keyLog, 1, _config);
                    return responseError;
                }

                sub_agent_user = miUtilerias.getPropertyValue("Header.sub_agent_user", JsonPartner);
                infoGeneralRemesador = miUtilerias.setRemesadorParameters(remesadorID, remesadorShortName, _config, operationID, countryCode, ref oError);
                if (infoGeneralRemesador.Count == 0 || !oError.Equals(string.Empty))
                {
                    responseRemesador = miUtilerias.BuildResponse(ACGError.RequestIsNullOrEmpty, "No se pudo obtener informacion basica del remesador " + oError, agency_code, sub_agent_user);
                    miUtilerias.SaveLog(responseRemesador, "1", keyLog, 1, _config);
                    return responseRemesador;
                }
                if (infoGeneralRemesador["CredencialUnicaRemesador"].Equals("True"))
                {
                    DataTable partnerCredentials;
                    if (new UtileriasProxy().getPropertyValue("header.agency_id", JObject.Parse(tramaPartner.ToString())) != null)
                    {
                        partnerCredentials = new ConexionBDProxy().GetPartnerUsuarioID(_config, Convert.ToInt32(miUtilerias.getPropertyValue("Header.agency_id", JObject.Parse(tramaDelPartner))), "", ref oError);
                    }
                    else
                    {
                        partnerCredentials = new ConexionBDProxy().GetPartnerUsuarioID(_config, 0, (miUtilerias.getPropertyValue("Header.agency_code", JObject.Parse(tramaDelPartner))), ref oError);
                    }
                    var validaCresd = (from row in partnerCredentials.AsEnumerable()
                                       select new
                                       {
                                           usuario = row.Field<string>("UsuarioPartner"),
                                           password = row.Field<string>("Password")
                                       }).FirstOrDefault();
                    infoGeneralRemesador["ServiceUser"] = validaCresd.usuario;
                    BFEngine oDecripKey = new BFEngine();
                    string oKey = oDecripKey.Process(oGeneral.mPasskey(), BFEngine.AccionCifrado.Decrypt);
                    //se encripta el psw
                    BFEngine oEncripTrama = new BFEngine(oKey);
                    infoGeneralRemesador["RemesadorServicePassword"] = oEncripTrama.Process(validaCresd.password, BFEngine.AccionCifrado.Decrypt);
                }

                //cuando es un SOAP ya no arma la trama aca sino en el servicio que ayuda a consumir el api del remesador
                if (infoGeneralRemesador["TipoServicio"].ToUpper() == "SOAP")
                {
                    responseRemesador = new UtileriasProxy().consumeServicioSoap(tramaDelPartner, keyLog, _config);
                    miUtilerias.SaveLog(responseRemesador, "1", keyLog, 1, _config);

                }
                else
                {
                    dtHeaders = new ConexionBDProxy().getRemesadorHeaderProperties(Convert.ToInt32(infoGeneralRemesador["remesador_id"]), countryCode, ref oError, _config);
                    CargarCertificados(infoGeneralRemesador, _config, dtHeaders);
                    generarWebRequest(dtHeaders);
                    miUtilerias.SaveLog("Empieza a construir Trama remesador Remesador: " + remesadorShortName + "operacion: " + operationID, "1", keyLog, 1, _config);
                    if (infoGeneralRemesador["TipoPeticion"].ToUpper() != "GET")
                    {
                        tramaRemesador = ConstruirTramaRemesador(_config, remesadorID, remesadorShortName, operationID, countryCode, JsonPartner, infoGeneralRemesador);
                        miUtilerias.SaveLog(tramaRemesador, "1", keyLog, 1, _config);
                    }
                    else
                    {
                        //construle una lista para los parametros GET
                        paramGetRest = ConsutruirLstParametrosRestGet(_config, remesadorID, remesadorShortName, operationID, countryCode, JsonPartner, infoGeneralRemesador);
                    }
                    responseRemesador = new UtileriasProxy().consumeServicioRest(tramaRemesador, infoGeneralRemesador, headers, paramGetRest);
                    miUtilerias.SaveLog(responseRemesador, "1", keyLog, 1, _config);
                }
            }
            catch (Exception ex)
            {
                responseRemesador = miUtilerias.BuildResponse(ACGError.RequestIsNullOrEmpty, ex.Message, agency_code, sub_agent_user);
                miUtilerias.SaveLog(responseRemesador, "1", keyLog, 1, _config);

            }
            return responseRemesador;
        }
        public dynamic ConstruirTramaRemesador(IConfiguration _config, int remesadorID, string remesadorShortName, string operationID, string countryCode, JObject JsonPartner, Dictionary<string, string> PrincipalObject = null, bool IsReplacingOperationID = false)
        {
            UtileriasProxy oUtil = new UtileriasProxy();
            DataTable oTableParamLocations = new DataTable();
            DataTable oTableCatalogos = new DataTable();
            DataTable oTableValoresDefault = new DataTable();
            dynamic TramaRequestRemesador = string.Empty;
            string oError = string.Empty;

            try
            {
                if (PrincipalObject == null)
                    PrincipalObject = oUtil.setRemesadorParameters(remesadorID, remesadorShortName, _config, operationID, countryCode, ref oError);

                var oTable = new ConexionBDProxy().getParamRemesador(remesadorID, operationID, remesadorShortName, ref oTableParamLocations, ref oTableValoresDefault, ref oTableCatalogos, ref oError, _config);
                var oTableObjectParents = oTable.AsEnumerable().Where(x => x.Field<int?>("ParametroPadre") == null).CopyToDataTable();
                switch (PrincipalObject["TipoServicio"].ToUpper())
                {
                    case "SOAP":
                        if (Convert.ToBoolean(PrincipalObject["TieneNameSpace"]))
                        {
                            DataTable NameSpaceTable = new DataTable();
                            DataTable NameSpaceParam = new DataTable();
                            Dictionary<int, XNamespace> LstNameSpace = new Dictionary<int, XNamespace>();
                            Dictionary<int, string> LstPrefix = new Dictionary<int, string>();
                            //SE LLAMA AL A LAS TABLAS DE DONDE SE JALA LOS NAMESTAPCE DE LOS NODOS
                            NameSpaceTable = new ConexionBDProxy().getNameSpaceRemesador(Convert.ToInt32(PrincipalObject["remesador_id"]), ref oError, _config);
                            NameSpaceParam = new ConexionBDProxy().getRemesadorParamNameSpace(Convert.ToInt32(PrincipalObject["remesador_id"]), ref oError, _config);
                            //SEPARA LOS NAMESPACE Y LOS PREFIJOS
                            foreach (DataRow item in NameSpaceTable.Rows)
                            {
                                XNamespace name = item["xmlNameSpace"].ToString();
                                LstNameSpace.Add(Convert.ToInt32(item["NameSpaceID"].ToString()), name);
                                LstPrefix.Add(Convert.ToInt32(item["NameSpaceID"].ToString()), item["XmlPrefix"].ToString());
                            }

                            foreach (DataRow item in oTableObjectParents.Rows)
                            {
                                XElement oDoc_Rest = oUtil.setXElement(NameSpaceParam, LstNameSpace, LstPrefix, Convert.ToInt32(item["ParametroRemesadorID"].ToString()), item["NombreParametro"].ToString());
                                //encuentra a los hijos
                                var oTableItems = oUtil.ValidateDataTable(oTable, Convert.ToInt32(item["ParametroRemesadorID"].ToString()));

                                foreach (DataRow subitem in oTableItems.Rows)
                                {
                                    XElement internalElemnt = oUtil.setXElement(NameSpaceParam, LstNameSpace, LstPrefix, Convert.ToInt32(subitem["ParametroRemesadorID"].ToString()), subitem["NombreParametro"].ToString());
                                    var oTableSubItems = oUtil.ValidateDataTable(oTable, Convert.ToInt32(subitem["ParametroRemesadorID"].ToString()));
                                    //arma la trama y asigna valores
                                    oUtil.CreateXmlRequest(NameSpaceParam, LstNameSpace, LstPrefix, Convert.ToInt32(subitem["ParametroRemesadorID"].ToString()), subitem["NombreParametro"].ToString(), oTable, oTableSubItems, oTableParamLocations, PrincipalObject, (JsonPartner), oTableCatalogos, oTableValoresDefault, ref internalElemnt);
                                    oDoc_Rest.Add(internalElemnt);
                                }
                                TramaRequestRemesador = oDoc_Rest;
                            }
                        }
                        else
                        {
                            foreach (DataRow item in oTableObjectParents.Rows)
                            {
                                XElement oDoc_Rest = new XElement(item["NombreParametro"].ToString());

                                var oTableItems = oUtil.ValidateDataTable(oTable, Convert.ToInt32(item["ParametroRemesadorID"].ToString()));

                                foreach (DataRow subitem in oTableItems.Rows)
                                {
                                    XElement internalElement = new XElement(subitem["NombreParametro"].ToString());
                                    var oTableSubItems = oUtil.ValidateDataTable(oTable, Convert.ToInt32(subitem["ParametroRemesadorID"].ToString()));
                                    oUtil.CreateXmlRequest(oTable, oTableSubItems, oTableParamLocations, PrincipalObject, (JsonPartner), oTableCatalogos, oTableValoresDefault, ref internalElement);
                                    oDoc_Rest.Add(internalElement);
                                }
                                TramaRequestRemesador = oDoc_Rest;
                            }
                        }
                        break;

                    case "REST":
                        switch (PrincipalObject["TipoRequestRemesador"].ToUpper())
                        {
                            case "JSON":
                                JObject json = new JObject();
                                foreach (DataRow item in oTableObjectParents.Rows)
                                {
                                    JObject internalObject = new JObject();
                                    var oTableSubItems = oUtil.ValidateDataTable(oTable, Convert.ToInt32(item["ParametroRemesadorID"].ToString()));
                                    oUtil.CreateJsonRequest(oTable, oTableSubItems, oTableParamLocations, PrincipalObject, JsonPartner, oTableValoresDefault, oTableCatalogos, ref internalObject);
                                    json.Add(item["NombreParametro"].ToString(), internalObject);
                                }
                                TramaRequestRemesador = json;
                                break;
                            case "XML":
                                if (Convert.ToBoolean(PrincipalObject["TieneNameSpace"]))
                                {
                                    DataTable NameSpaceTable = new DataTable();
                                    DataTable NameSpaceParam = new DataTable();
                                    Dictionary<int, XNamespace> LstNameSpace = new Dictionary<int, XNamespace>();
                                    Dictionary<int, string> LstPrefix = new Dictionary<int, string>();
                                    //SE LLAMA AL A LAS TABLAS DE DONDE SE JALA LOS NAMESTAPCE DE LOS NODOS
                                    NameSpaceTable = new ConexionBDProxy().getNameSpaceRemesador(Convert.ToInt32(PrincipalObject["remesador_id"]), ref oError, _config);
                                    NameSpaceParam = new ConexionBDProxy().getRemesadorParamNameSpace(Convert.ToInt32(PrincipalObject["remesador_id"]), ref oError, _config);
                                    //SEPARA LOS NAMESPACE Y LOS PREFIJOS
                                    foreach (DataRow item in NameSpaceTable.Rows)
                                    {
                                        XNamespace name = item["xmlNameSpace"].ToString();
                                        LstNameSpace.Add(Convert.ToInt32(item["NameSpaceID"].ToString()), name);
                                        LstPrefix.Add(Convert.ToInt32(item["NameSpaceID"].ToString()), item["XmlPrefix"].ToString());
                                    }

                                    foreach (DataRow item in oTableObjectParents.Rows)
                                    {
                                        XElement oDoc_Rest = oUtil.setXElement(NameSpaceParam, LstNameSpace, LstPrefix, Convert.ToInt32(item["ParametroRemesadorID"].ToString()), item["NombreParametro"].ToString());
                                        //encuentra a los hijos
                                        var oTableItems = oUtil.ValidateDataTable(oTable, Convert.ToInt32(item["ParametroRemesadorID"].ToString()));

                                        foreach (DataRow subitem in oTableItems.Rows)
                                        {
                                            XElement internalElemnt = oUtil.setXElement(NameSpaceParam, LstNameSpace, LstPrefix, Convert.ToInt32(subitem["ParametroRemesadorID"].ToString()), subitem["NombreParametro"].ToString());
                                            var oTableSubItems = oUtil.ValidateDataTable(oTable, Convert.ToInt32(subitem["ParametroRemesadorID"].ToString()));
                                            //arma la trama
                                            oUtil.CreateXmlRequest(NameSpaceParam, LstNameSpace, LstPrefix, Convert.ToInt32(subitem["ParametroRemesadorID"].ToString()), subitem["NombreParametro"].ToString(), oTable, oTableSubItems, oTableParamLocations, PrincipalObject, (JsonPartner), oTableCatalogos, oTableValoresDefault, ref internalElemnt);
                                            foreach (var nodo in internalElemnt.Descendants())
                                            {
                                                if(nodo.Name.Namespace == "")
                                                {
                                                    nodo.Attributes("xmlns").Remove();
                                                    nodo.Name = nodo.Parent.Name.Namespace + nodo.Name.LocalName;

                                                }
                                            }
                                            oDoc_Rest.Add(internalElemnt);
                                        }
                                        TramaRequestRemesador = oDoc_Rest;
                                    }
                                }
                                else
                                {
                                    foreach (DataRow item in oTableObjectParents.Rows)
                                    {
                                        XElement oDoc_Rest = new XElement(item["NombreParametro"].ToString());

                                        var oTableItems = oUtil.ValidateDataTable(oTable, Convert.ToInt32(item["ParametroRemesadorID"].ToString()));

                                        foreach (DataRow subitem in oTableItems.Rows)
                                        {
                                            XElement internalElement = new XElement(subitem["NombreParametro"].ToString());
                                            var oTableSubItems = oUtil.ValidateDataTable(oTable, Convert.ToInt32(subitem["ParametroRemesadorID"].ToString()));
                                            oUtil.CreateXmlRequest(oTable, oTableSubItems, oTableParamLocations, PrincipalObject, (JsonPartner), oTableCatalogos, oTableValoresDefault, ref internalElement);
                                            oDoc_Rest.Add(internalElement);
                                        }
                                        TramaRequestRemesador = oDoc_Rest;
                                    }
                                }
                                break;
                        }
                        break;
                }
                return TramaRequestRemesador;
            }
            catch (Exception ex) { throw ex; }
        }
        public void CargarCertificados(Dictionary<string, string> infoGeneralRemesador, IConfiguration _config, DataTable headersRemesador)
        {
            string token = string.Empty;
            X509Certificate certData;
            Dictionary<string, string> headersCertficiados = new Dictionary<string, string>();
            try
            {
                if (infoGeneralRemesador["TipoCredencial"].ToUpper() == "JWT")
                {
                    token = new UtileriasProxy().GenerarJWT(_config, infoGeneralRemesador, headersRemesador);
                    headers.Headers.Add(HttpRequestHeader.Authorization, "Bearer " + token);
                }
                if (infoGeneralRemesador["TipoCredencial"].ToUpper() == "CERTIFICADO")
                {
                    certData = new UtileriasProxy().GetCertData(Convert.ToInt32(infoGeneralRemesador["remesador_id"]), _config);
                    headers.cert = certData;
                }

                if (infoGeneralRemesador["TipoCredencial"].ToUpper() == "KEY")
                {
                    string keyRemesador = infoGeneralRemesador["key"];
                    headers.Headers.Add(HttpRequestHeader.Authorization, "Bearer " + keyRemesador);
                }

                if (infoGeneralRemesador["TipoCredencial"].ToUpper() == "CREDENCIAL")
                {
                    string Credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes(infoGeneralRemesador["ServiceUser"] + ":" + infoGeneralRemesador["RemesadorServicePassword"]));
                    headers.Headers.Add(HttpRequestHeader.Authorization, "Basic " + Credentials);
                }
                //se carga certificado y credencial
                if (infoGeneralRemesador["TipoCredencial"].ToUpper() == "CERTIFICADO" && infoGeneralRemesador["ServiceUser"] != "")
                {
                    certData = new UtileriasProxy().GetCertData(Convert.ToInt32(infoGeneralRemesador["remesador_id"]), _config);
                    string Credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes(infoGeneralRemesador["ServiceUser"] + ":" + infoGeneralRemesador["RemesadorServicePassword"]));
                    headers.Headers.Add(HttpRequestHeader.Authorization, "Basic " + Credentials);

                    headers.cert = certData;

                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public void generarWebRequest(DataTable infoHeaders)
        {
            //aca se asigna a los headers que se recuperan en la BD
            using (var client = new MyWebClient())
            {
                foreach (DataRow item in infoHeaders.Rows)
                {
                    if (item["EsHeader"].ToString().ToUpper() == "TRUE")
                    {
                        headers.Headers.Add(item["property"].ToString(), item["value"].ToString());
                    }
                }
            }
        }
        public Dictionary<string, string> ConsutruirLstParametrosRestGet(IConfiguration _config, int remesadorID, string remesadorShortName, string operationID, string countryCode, JObject JsonPartner, Dictionary<string, string> PrincipalObject = null, bool IsReplacingOperationID = false)
        {
            DataTable oTableParamLocations = new DataTable();
            string oError = string.Empty;
            string partnerLocation = string.Empty;
            string valorTramaPartner = string.Empty;
            DataTable oTableCatalogos = new DataTable();
            DataTable oTableValoresDefault = new DataTable();
            var oTable = new ConexionBDProxy().getParamRemesador(remesadorID, operationID, remesadorShortName, ref oTableParamLocations, ref oTableValoresDefault, ref oTableCatalogos, ref oError, _config);
            var oTableObjectParents = oTable.AsEnumerable().Where(x => x.Field<int?>("ParametroPadre") == null).CopyToDataTable();
            Dictionary<string, string> allIputParams = new Dictionary<string, string>();
            try
            {
                foreach (DataRow itemRuta in oTableParamLocations.Rows)
                {
                    partnerLocation = itemRuta["partnerlocation"].ToString();
                    if (partnerLocation.Split('.')[0].ToUpper().Equals("OPERATIONID"))
                    {
                        partnerLocation = partnerLocation.Replace(partnerLocation.Split('.')[0], PrincipalObject["operation_id"]);
                    }

                    valorTramaPartner = new UtileriasProxy().getPropertyValue(partnerLocation, JsonPartner);
                    allIputParams.Add(itemRuta["RemesadorLocation"].ToString(), valorTramaPartner);
                }
                return allIputParams;
            }
            catch (Exception ex) { throw ex; }

        }
    }
}
