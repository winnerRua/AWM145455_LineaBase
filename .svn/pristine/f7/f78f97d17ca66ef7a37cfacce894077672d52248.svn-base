
using GrupoCoen.Corporativo.Libraries.AH2HAS.Operaciones.Operadores;
using GrupoCoen.Corporativo.ProjectCenter.Libraries.CriptoEngine;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using WSREGGWMM.Entities;
using WSREGGWMM.Helpers;
using WSREGPROXY.Services;
using SymmetricSecurityKey = Microsoft.IdentityModel.Tokens.SymmetricSecurityKey;

namespace WSREGGWMM.Services
{
    public class Utilerias
    {
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
        internal string GenerateToken(int Lenght, int NonAlphaNumericChars)
        {
            string allowedChars = "abcdefghijkmnopqrstuvwxyzABCDEFGHJKLMNOPQRSTUVWXYZ0123456789";
            string allowedNonAlphaNum = "!@#$%^&*()_-+=[{]};:<>|./?";
            Random rd = new Random();

            if (NonAlphaNumericChars > Lenght || Lenght <= 0 || NonAlphaNumericChars < 0)
                throw new ArgumentOutOfRangeException();

            char[] pass = new char[Lenght];
            int[] pos = new int[Lenght];
            int i = 0, j = 0, temp = 0;
            bool flag = false;

            //Random the position values of the pos array for the string Pass
            while (i < Lenght - 1)
            {
                j = 0;
                flag = false;
                temp = rd.Next(0, Lenght);
                for (j = 0; j < Lenght; j++)
                    if (temp == pos[j])
                    {
                        flag = true;
                        j = Lenght;
                    }

                if (!flag)
                {
                    pos[i] = temp;
                    i++;
                }
            }

            //Random the AlphaNumericChars
            for (i = 0; i < Lenght - NonAlphaNumericChars; i++)
                pass[i] = allowedChars[rd.Next(0, allowedChars.Length)];

            //Random the NonAlphaNumericChars
            for (i = Lenght - NonAlphaNumericChars; i < Lenght; i++)
                pass[i] = allowedNonAlphaNum[rd.Next(0, allowedNonAlphaNum.Length)];

            //Set the sorted array values by the pos array for the rigth posistion
            char[] sorted = new char[Lenght];
            for (i = 0; i < Lenght; i++)
                sorted[i] = pass[pos[i]];

            string Pass = new String(sorted);

            return Pass;
        }
        internal string GenerarJWT(IConfiguration _config)
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
        internal void SaveLog(dynamic pMetadata, string identificadorEvento, string keyEvent, int ComponentID, IConfiguration _config)
        {

            try
            {
                string oError = string.Empty;
                SaveLogDataDelegate.SaveLogDataDelegateAsync(pMetadata, identificadorEvento, keyEvent, ComponentID, _config);
            }
            catch
            {
            }
        }
        internal bool setPartnerParameters(int PartnerID, ref Dictionary<string, string> auxObject, IConfiguration _config)
        {
            try
            {
                ConexionBD oData = new ConexionBD();
                string errorMessage = string.Empty;
                var oTable = oData.get_PartnerConfig(PartnerID, ref errorMessage, _config);
                bool response = false;

                foreach (DataRow item in oTable.Rows)
                {
                    auxObject.Add("PartnerServiceType", item["TipoServicio"].ToString());
                    auxObject.Add("PartnerConfigID", item["PartnerConfigID"].ToString());
                    auxObject.Add("PartnerRequestType", item["TipoRequestPartner"].ToString());
                    auxObject.Add("hasNameSpace", item["TieneNameSpace"].ToString());
                    response = true;
                }

                return response;
            }
            catch
            {
                return false;
            }
        }
        internal void CreateJsonRequest(DataTable oTable, DataTable oTableSubItems, ref JObject internalObject)
        {
            foreach (DataRow subitem in oTableSubItems.Rows)
            {
                var TipoParam = subitem["TipoParametro"].ToString().ToUpper().Trim();
                switch (TipoParam)
                {
                    case "OBJECT":
                        JObject auxJson = new JObject();
                        var oTableItems = TraerNodosHijo(oTable, Convert.ToInt32(subitem["ParametroPartnerID"].ToString()));
                        CreateJsonRequest(oTable, oTableItems, ref auxJson);
                        internalObject.Add(subitem["NombreParametro"].ToString(), auxJson);
                        break;
                    case "STRING":
                        internalObject.Add(subitem["NombreParametro"].ToString(), "");
                        break;
                }
            }
        }
        internal DataTable TraerNodosHijo(DataTable oTable, int ParamID)
        {
            try
            {
                if (oTable.AsEnumerable().Where(x => x.Field<int?>("ParametroPadre") != null && x.Field<int?>("ParametroPadre") == ParamID && x.Field<bool?>("EsRequerido") == true).Count() > 0)
                {
                    dynamic oTableItems = oTable.AsEnumerable().Where(x => x.Field<int?>("ParametroPadre") != null && x.Field<int?>("ParametroPadre") == ParamID && x.Field<bool?>("EsRequerido") == true).CopyToDataTable();
                    if (oTableItems.Rows.Count > 0)
                        oTableItems = oTable.AsEnumerable().Where(x => x.Field<int?>("ParametroPadre") != null && x.Field<int?>("ParametroPadre") == ParamID && x.Field<bool?>("EsRequerido") == true).CopyToDataTable();
                    else
                        oTableItems = new DataTable();
                    return oTableItems;
                }
                else return new DataTable();
            }
            catch
            {
                return new DataTable();
            }
        }
        internal JObject ErrorResponse(string oError, dynamic data, dynamic errorDetails)
        {

            try
            {
                JObject objeto = new JObject();
                JObject jsonData = new JObject();
                int partnerID = 0;

                string value = string.Empty;
                List<string> nodos = new List<string>();
                Dictionary<string, string> nodosValor = new Dictionary<string, string>();
                jsonData = JObject.Parse(Convert.ToString(data));

                nodos.Add("faultactor");
                nodos.Add("error");
                nodos.Add("ExternalReferenceNumber");
                foreach (var nodo in nodos)
                {
                    encontrarValorNodo(jsonData, nodo, ref value);
                    var valor = value;
                    nodosValor.Add(nodo, valor);
                    value = string.Empty;

                }

                ErrorGateway error = new ErrorGateway
                {
                    faultcode = "AIRPAK-ERROR",
                    faultactor = nodosValor["faultactor"],
                    faultstring = oError,
                    detail = new Detail
                    {
                        airpak_error =
                                new Airpak_error
                                {
                                    error = errorDetails,
                                    ExternalReferenceNumber = nodosValor["ExternalReferenceNumber"],
                                    PartnerID = "1"
                                }
                    }
                };
                var subobjeto = JObject.Parse(JsonConvert.SerializeObject(error));
                objeto.Add("Fault", subobjeto);
                return objeto;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        internal void encontrarValorNodo(JObject jObject, string nodo, ref string value)
        {
            foreach (var jitem in jObject)
            {
                if (jitem.Value.Type == JTokenType.Object)
                {
                    //jsonParents.Add(jitem.Key);
                    if (jitem.Key.ToString() == nodo)
                    {
                        value = jitem.Value.ToString();
                        break;
                    }
                    var jSubObject = (JObject)jitem.Value;
                    foreach (var jsubitem in jSubObject)
                    {
                        if (jsubitem.Key.ToString() == nodo)
                        {
                            value = jsubitem.Value.ToString();
                            break;
                        }
                        //Se va evaluando pro el valor del key y si este es objeto pasa sino escribe
                        if (jsubitem.Value.Type == JTokenType.Object)
                        {
                            encontrarValorNodo((JObject)jsubitem.Value, nodo, ref value);
                        }
                        else
                        {
                            if (jsubitem.Key.ToString() == nodo)
                            {
                                value = jsubitem.Value.ToString();
                                break;

                            }
                        }
                    }
                }
                else
                {
                    if (jitem.Key.ToString() == nodo)
                    {
                        value = jitem.Value.ToString();
                        break;

                    }
                }
            }

        }
        public bool ValidateCredentials(IConfiguration _config, string operador, string password, int IdAgencia, int IdPais, string isoCode,string idproducto, string strRequest, ref string respuesta)
        {
            ConexionBD conexionDB = new ConexionBD();
            try
            {
                string idproductosyr = "";
                bool userOK = false;

                try
                {
                    idproductosyr = new Utilerias().getAppSettingsKey("KeySYR", _config);
                }
                catch (Exception ex)
                {
                    respuesta = "No se encontró el Key de Simple y Rápido: KeySYR, en el archivo de configuración";
                    return false;
                }
                //Validamos si el producto corresponde a Simple y Rápido
                if (!idproducto.Trim().ToLower().Equals(idproductosyr.Trim().ToLower()))
                    userOK = conexionDB.mValidarOp(_config,operador, password, IdAgencia.ToString(), idproducto, isoCode, ref respuesta);

                else
                {
                    //Si es Simple y Rápido, las cuenta de Rol Operador CallCenter/Operador Común
                    userOK = conexionDB.mValidarOpSyR(_config,operador, password, IdAgencia.ToString(), IdPais, idproducto, ref respuesta);
                }

                return userOK;

            }
            catch (Exception ex)
            {
                respuesta = ex.Message;
                return false;

            }
        }
    }
}
