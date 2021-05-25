using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using WSREGGWMM.Services;
using WSREGPROXY.Helpers;

namespace WSREGGWMM.Helpers
{
    public class ValidarNodos
    {

        private readonly IConfiguration _config;

        public ValidarNodos(IConfiguration config)
        {
            _config = config;
        }
        internal string validaNodos(string jsonPartner, string strLogKey, string operacionID, int PartnerID, string productShortName, int remesadorID ,ref List<NodoExcepcion> nodosInvalidos)
        {
            #region Variables
            string respuesta = string.Empty, response = string.Empty; 
            Utilerias utilerias = new Utilerias();
            JObject jObject;
            #endregion

            try
            {
                jObject = JObject.Parse(jsonPartner.Replace("-","_"));

                List<dynamic> jsonParents = new List<dynamic>();
                List<dynamic> jsonChilds = new List<dynamic>();
                //se sacan los padres e hijos del json
                sacarNodosJson(jObject, ref jsonParents, ref jsonChilds);
                string oError = string.Empty;
                var oTable = new ConexionBD().get_ServiceParameters(PartnerID, operacionID,productShortName, remesadorID, ref oError, _config);
                if (oError != "")
                {
                    return oError;
                }

                var oTableObjectsPadre = oTable.AsEnumerable().Where(x => x.Field<string>("TipoParametro") == "object" && x.Field<bool?>("EsRequerido") == true).CopyToDataTable();
                //se recorre los nodos padre y se busca si existen en la trama json que viene
                foreach (DataRow item in oTableObjectsPadre.Rows)
                {
                    if (jsonParents.Exists(x => x.Key.ToLower().Replace("-","_") == item["NombreParametro"]?.ToString()?.ToLower().Replace("-", "_")))
                    {
                        //se valida que el nodo hijo exista en la trama json que viene
                        var oTableSubItems = utilerias.TraerNodosHijo(oTable, Convert.ToInt32(item["ParametroPartnerID"].ToString()));
                        
                        foreach (var itemSubTable in oTableSubItems.AsEnumerable())
                        {
                            //si el nodo hijo es un objeto se busca en la lista de nodos padre
                            if (itemSubTable["TipoParametro"].ToString() == "object")
                            {
                                var valorTramaPartner = new UtileriasProxy().getPropertyValue(itemSubTable["partnerLocation"].ToString().ToLower().Replace("operationid", operacionID.Replace("-","_")), jObject);

                                if (valorTramaPartner == null && itemSubTable["EsRequerido"].ToString().CompareTo("True") == 0)
                                {
                                    NodoExcepcion nodo = new NodoExcepcion();
                                    nodo.Nodo = itemSubTable["NombreParametro"].ToString();
                                    nodo.Ubicacion = itemSubTable["partnerLocation"].ToString().ToLower().Replace("operationid", operacionID.Replace("-", "_")) ?? "";
                                    nodo.Mensaje = "Nodo Faltante";
                                    nodosInvalidos.Add(nodo);
                                }
                                    
                                else if ((string.IsNullOrEmpty(valorTramaPartner) || valorTramaPartner == "{}" || valorTramaPartner == "") && itemSubTable["RequiereValor"].ToString().CompareTo("True") == 0)
                                {
                                    NodoExcepcion nodo = new NodoExcepcion();
                                    nodo.Nodo = itemSubTable["NombreParametro"].ToString();
                                    nodo.Ubicacion = itemSubTable["partnerLocation"].ToString().ToLower().Replace("operationid", operacionID.Replace("-", "_")) ?? "";
                                    nodo.Mensaje = "Nodo no puede ser vacio";
                                    nodosInvalidos.Add(nodo);
                                }
                            }
                            else
                            {
                                var valorTramaPartner = new UtileriasProxy().getPropertyValue(itemSubTable["partnerLocation"].ToString().ToLower().Replace("operationid", operacionID.Replace("-", "_")), jObject);

                                if (valorTramaPartner == null && itemSubTable["EsRequerido"].ToString().CompareTo("True") == 0)
                                {
                                    NodoExcepcion nodo = new NodoExcepcion();
                                    nodo.Nodo = itemSubTable["NombreParametro"].ToString();
                                    nodo.Ubicacion = itemSubTable["partnerLocation"].ToString().ToLower().Replace("operationid", operacionID.Replace("-", "_")) ?? "";
                                    nodo.Mensaje = "Nodo Faltante";
                                    nodosInvalidos.Add(nodo);
                                }

                                else if (string.IsNullOrEmpty(valorTramaPartner) && itemSubTable["RequiereValor"].ToString().CompareTo("True") == 0)
                                {
                                    NodoExcepcion nodo = new NodoExcepcion();
                                    nodo.Nodo = itemSubTable["NombreParametro"].ToString();
                                    nodo.Ubicacion = itemSubTable["partnerLocation"].ToString().ToLower().Replace("operationid", operacionID.Replace("-", "_")) ?? "";
                                    nodo.Mensaje = "Nodo no puede ser vacio";
                                    nodosInvalidos.Add(nodo);
                                }
                            }
                        }
                    }
                    else
                    {
                        NodoExcepcion nodo = new NodoExcepcion();
                        nodo.Nodo = item["NombreParametro"].ToString();
                        nodo.Ubicacion = item["partnerLocation"].ToString().ToLower().Replace("operationid", operacionID.Replace("-", "_")) ?? "";
                        nodo.Mensaje = "Nodo Faltante";
                        nodosInvalidos.Add(nodo);
                    }
                }
                response = nodosInvalidos.Count() > 0 ? "Request es invalido, revisar detalle de campos" : "";
                utilerias.SaveLog(response, "1", strLogKey, 1, _config);

            }
            catch (Exception ex)
            {
                throw ex;
            }

            return response;
        }
        internal void sacarNodosJson(JObject jObject, ref List<dynamic> jsonParents, ref List<dynamic> jsonChilds)
        {
            foreach (var jitem in jObject)
            {
                if (jitem.Value.Type == JTokenType.Object)
                {
                    jsonParents.Add(jitem);
                    var jSubObject = (JObject)jitem.Value;
                    foreach (var jsubitem in jSubObject)
                    {
                        //Se va evaluando pro el valor del key y si este es objeto pasa sino escribe
                        if (jsubitem.Value.Type == JTokenType.Object)
                        {
                            jsonParents.Add(jsubitem);
                            sacarNodosJson((JObject)jsubitem.Value, ref jsonParents, ref jsonChilds);
                        }
                        else
                        {
                            jsonChilds.Add(jsubitem);
                        }
                    }
                }
                else
                {
                    jsonChilds.Add(jitem);
                }
            }
        }
    }

    public class NodoExcepcion
    {
        public string Nodo { get; set; }
        public string Ubicacion { get; set; }
        public string Mensaje{ get; set; }
    }
}
