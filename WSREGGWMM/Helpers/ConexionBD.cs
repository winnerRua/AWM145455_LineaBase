using GrupoCoen.Corporativo.Libraries.AH2HAS.Operaciones.Operadores;
using GrupoCoen.Corporativo.Libraries.AH2HAS.Utilidades;
using GrupoCoen.Corporativo.Libraries.ConexionBD;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using WSREGGWMM.Services;
using System.Linq;

namespace WSREGGWMM.Helpers
{
    public class ConexionBD
    {
        Utilerias oUtilerias = new Utilerias();
        MSSQLConnection ocon;
        AS_Generales oGeneral;

        string oCS = String.Empty;
        string Procedure = String.Empty;

        internal DataTable GetPartnerUsuario(IConfiguration _config, int agencyId,string agencyCode, ref string oError, string operador = "", string producto = "", string paisValidacion = "")
        {
            try
            {
                DataSet oDT = new DataSet();
                DataTable oTable = new DataTable();

                MSSQLConnection ocon = new MSSQLConnection();
                var oCS = string.Empty;
                var Procedure = string.Empty;
                var lstString = new List<string>();
                bool executeResult = false;

                oCS = new Utilerias().getAppSettingsKey("dbalias", _config);
                Procedure = new Utilerias().getAppSettingsKey("GetPartnerUsuario", _config, "StoredProcedures");
                ocon = new MSSQLConnection(oCS);

                List<Parametros> iParams = new List<Parametros>();
                iParams.Add(new Parametros("@Agency_id", agencyId, Parametros.SType.Int));
                iParams.Add(new Parametros("@operador", operador, Parametros.SType.VarChar));
                iParams.Add(new Parametros("@producto", producto, Parametros.SType.VarChar));
                iParams.Add(new Parametros("@PaisValidacion", paisValidacion, Parametros.SType.VarChar));
                iParams.Add(new Parametros("@agency_code", agencyCode, Parametros.SType.VarChar));

                try
                {
                    executeResult = ocon.executeSP(Procedure, iParams, MSSQLConnection.ReturnTypes.Dataset);
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
                    ocon.closeConnection();
                }

                oDT = ocon.getDataset();

                if (oDT.Tables.Count == 0)
                {
                    oError = "La busqueda no obtuvo ningún dato de Partner";
                    throw new Exception();
                }

                oTable = oDT.Tables[0];

                if (oTable.Rows.Count == 0)
                {
                    oError = "La busqueda no obtuvo ningún dato de Partner";
                    throw new Exception();
                }

                return oTable;
            }
            catch (Exception oex)
            {
                oError = oError + " / " + oex.Message;
                return new DataTable();
            }
        }
        internal int GetPartnerUsuarioID(IConfiguration _config, ref string oError, int agencyID = 0 , string agencyCode = "")
        {
            try
            {
                DataSet oDT = new DataSet();
                DataTable oTable = new DataTable();

                MSSQLConnection ocon = new MSSQLConnection();
                var oCS = string.Empty;
                var Procedure = string.Empty;
                var lstString = new List<string>();
                bool executeResult = false;

                oCS = new Utilerias().getAppSettingsKey("dbalias", _config);
                Procedure = new Utilerias().getAppSettingsKey("GetPartnerUsuario", _config, "StoredProcedures");
                ocon = new MSSQLConnection(oCS);

                List<Parametros> iParams = new List<Parametros>();
                iParams.Add(new Parametros("@Agency_id", agencyID, Parametros.SType.Int));
                iParams.Add(new Parametros("@operador", "", Parametros.SType.VarChar));
                iParams.Add(new Parametros("@producto", "", Parametros.SType.VarChar));
                iParams.Add(new Parametros("@PaisValidacion", "", Parametros.SType.VarChar));
                iParams.Add(new Parametros("@agency_code", agencyCode, Parametros.SType.VarChar));
                
                try
                {
                    executeResult = ocon.executeSP(Procedure, iParams, MSSQLConnection.ReturnTypes.Dataset);
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
                    ocon.closeConnection();
                }

                oDT = ocon.getDataset();

                if (oDT.Tables.Count == 0)
                {
                    oError = "La busqueda no obtuvo ningún dato de Partner";
                    throw new Exception();
                }

                oTable = oDT.Tables[0];

                if (oTable.Rows.Count == 0)
                {
                    oError = "La busqueda no obtuvo ningún dato de Partner";
                    throw new Exception();
                }
                int partnerID = 0;
                foreach (DataRow item in oTable.Rows)
                {
                    partnerID = Convert.ToInt32(item["partnerID"]);
                }

                return partnerID;
            }
            catch (Exception oex)
            {
                oError = oError + " / " + oex.Message;
                return 0;
            }
        }
        internal DataTable GetAutorizacionTradicional(IConfiguration _config, string agencyCode, string user, string password, ref string oError)
        {
            try
            {
                DataSet oDT = new DataSet();
                DataTable oTable = new DataTable();
                bool ob = false;

                oCS = new Utilerias().getAppSettingsKey("dbalias", _config, "AppSettings");
                Procedure = new Utilerias().getAppSettingsKey("getPartnerServiceParameter", _config, "StoredProcedures");
                ocon = new MSSQLConnection(oCS);

                List<Parametros> oParam = new List<Parametros>();
                //oParam.Add(new Parametros("@PartnerID", PartnerID, Parametros.SType.Int));
                //oParam.Add(new Parametros("@OperacionID", OperacionID, Parametros.SType.VarChar));
                //oParam.Add(new Parametros("@ProductShortName", productShortName, Parametros.SType.VarChar));
                //oParam.Add(new Parametros("@RemesadorID", remesadorID, Parametros.SType.Int));

                try
                {
                    ob = ocon.executeSP(Procedure, oParam, MSSQLConnection.ReturnTypes.Dataset);
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
                    ocon.closeConnection();
                }

                oDT = ocon.getDataset();

                if (oDT.Tables.Count == 0)
                {
                    oError = "La busqueda no obtuvo ningún parámetro de servicio";
                }

                oTable = oDT.Tables[0];

                if (oTable.Rows.Count == 0)
                {
                    oError = "La busqueda no obtuvo ningún parámetro de servicio";
                }

                return oTable;
            }
            catch (Exception oex)
            {
                oError = oError + " / " + oex.Message;
            }
            return new DataTable();


        }
        internal DataTable get_PartnerConfig(int PartnerID, ref string oError, IConfiguration _config)
        {
            try
            {
                DataSet oDT = new DataSet();
                DataTable oTable = new DataTable();
                bool ob = false;

                oCS = new Utilerias().getAppSettingsKey("dbalias", _config, "AppSettings");
                Procedure = new Utilerias().getAppSettingsKey("getpartnerconfig", _config, "StoredProcedures");
                ocon = new MSSQLConnection(oCS);

                List<Parametros> oParam = new List<Parametros>();
                oParam.Add(new Parametros("@PartnerID", PartnerID, Parametros.SType.Int));

                try
                {
                    ob = ocon.executeSP(Procedure, oParam, MSSQLConnection.ReturnTypes.Dataset);
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
                    ocon.closeConnection();
                }

                oDT = ocon.getDataset();

                if (oDT.Tables.Count == 0)
                {
                    oError = "La busqueda no obtuvo ningún dato de Partner";
                }

                oTable = oDT.Tables[0];

                if (oTable.Rows.Count == 0)
                {
                    oError = "La busqueda no obtuvo ningún dato de Partner";
                }

                return oTable;
            }
            catch (Exception oex)
            {
                oError = oError + " / " + oex.Message;
            }
            return new DataTable();
        }
        internal DataTable get_ServiceParameters(int PartnerID, string OperacionID, string productShortName, int remesadorID, ref string oError, IConfiguration _config)
        {
            DataSet oDT = new DataSet();
            DataTable oTable = new DataTable();
            bool ob = false;

            try
            {
                oCS = new Utilerias().getAppSettingsKey("dbalias", _config, "AppSettings");
                Procedure = new Utilerias().getAppSettingsKey("getPartnerServiceParameter", _config, "StoredProcedures");
                ocon = new MSSQLConnection(oCS);

                List<Parametros> oParam = new List<Parametros>();
                oParam.Add(new Parametros("@PartnerID", PartnerID, Parametros.SType.Int));
                oParam.Add(new Parametros("@OperacionID", OperacionID, Parametros.SType.VarChar));
                oParam.Add(new Parametros("@ProductShortName", productShortName, Parametros.SType.VarChar));
                oParam.Add(new Parametros("@RemesadorID", remesadorID, Parametros.SType.Int));

                try
                {
                    ob = ocon.executeSP(Procedure, oParam, MSSQLConnection.ReturnTypes.Dataset);
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
                    ocon.closeConnection();
                }

                oDT = ocon.getDataset();

                if (oDT.Tables.Count == 0)
                {
                    oError = "La busqueda no obtuvo ningún parámetro de servicio";
                }

                oTable = oDT.Tables[0];

                if (oTable.Rows.Count == 0)
                {
                    oError = "La busqueda no obtuvo ningún parámetro de servicio";
                }

                return oTable;
            }
            catch (Exception oex)
            {
                oError = oError + " / " + oex.Message;
            }
            return new DataTable();

        }
        internal int get_paisID(IConfiguration _config, string isoCode, ref string oError)
        {

            try
            {
                DataSet oDT = new DataSet();
                DataTable oTable = new DataTable();

                MSSQLConnection ocon = new MSSQLConnection();
                var oCS = string.Empty;
                var Procedure = string.Empty;
                var lstString = new List<string>();
                bool executeResult = false;

                oCS = new Utilerias().getAppSettingsKey("dbalias", _config);
                Procedure = new Utilerias().getAppSettingsKey("getIdPais", _config, "StoredProcedures");
                ocon = new MSSQLConnection(oCS);

                List<Parametros> iParams = new List<Parametros>();
                iParams.Add(new Parametros("@codigoIso", isoCode, Parametros.SType.VarChar));

                try
                {
                    executeResult = ocon.executeSP(Procedure, iParams, MSSQLConnection.ReturnTypes.Dataset);
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
                    ocon.closeConnection();
                }

                oDT = ocon.getDataset();

                if (oDT.Tables.Count == 0)
                {
                    oError = "La busqueda no obtuvo ningún dato de Pais";
                    throw new Exception();
                }

                oTable = oDT.Tables[0];

                if (oTable.Rows.Count == 0)
                {
                    oError = "La busqueda no obtuvo ningún dato de Pais";
                    throw new Exception();
                }
                int paisID = 0;
                foreach (DataRow item in oTable.Rows)
                {
                    paisID = Convert.ToInt32(item["PaisID"]);
                }

                return paisID;
            }
            catch (Exception oex)
            {
                oError = oError + " / " + oex.Message;
                return 0;
            }

        }
        public bool mValidarOp(IConfiguration _config, string oOperador, string oPsw, string oAgencia, string oProducto, string isoCode, ref string oError)
        {
            bool userOK = false;
            string oCT = "";
            DataSet oDT = new DataSet();
            DataTable oTable = new DataTable();
            string oPswDB = "";
            string oPswEncrip = "";

            try
            {
                oGeneral = new AS_Generales();
                oCS = new Utilerias().getAppSettingsKey("dbalias", _config, "AppSettings");
                ocon = new MSSQLConnection(oCS);
                Procedure = new Utilerias().getAppSettingsKey("getCredencialesOperador", _config, "StoredProcedures");
                List<Parametros> oParam = new List<Parametros>();
                oParam.Add(new Parametros("@operador", oOperador, Parametros.SType.NVarChar));
                oParam.Add(new Parametros("@agencia", oAgencia, Parametros.SType.Int));
                oParam.Add(new Parametros("@producto", oProducto, Parametros.SType.VarChar));
                oParam.Add(new Parametros("@PaisValidacion", isoCode, Parametros.SType.VarChar));

                bool ob = false;
                try
                {
                    ob = ocon.executeSP(Procedure, oParam, MSSQLConnection.ReturnTypes.Dataset);

                }
                catch (SqlException oExQ)
                {

                    oError = oError + " /  " + oExQ.Message;
                    return false;

                }
                catch (Exception oEx)
                {

                    oError = oError + " /  " + oEx.Message;
                    return false;
                }
                #region resultado nulo
                if (!ob)
                {
                    oError = "Error de ejecución SP al momento de obtener las credenciales del operador";
                    return false;
                }
                #endregion

                oDT = ocon.getDataset();
                oTable = oDT.Tables[0];

                if (oTable.Rows.Count == 0)
                {
                    oError = "No se encuentran operadores con estas credenciales";
                    return false;
                }
                else if (oTable.Columns.IndexOf("msj") == 0)
                {
                    oError = oTable.Rows[0]["msj"].ToString();
                    return false;
                }

                oPswDB = oTable.Rows[0]["psw"].ToString();

                oPswEncrip = new AS_Operadores().mDecriptarPsw(oPswDB, ref oError);

                if (oPsw.Equals(oPswEncrip))
                {
                    userOK = true;
                }
                else
                {
                    userOK = false;
                }

            }
            catch (Exception oex)
            {

                oError = oError + "/" + oex.Message;
                userOK = false;
            }

            return userOK;
        }

        public bool mValidarOpSyR(IConfiguration _config, string oOperador, string oPsw, string oAgencia, int IdPais, string oProducto, ref string oError)
        {
            bool userOK = false;
            string oCT = "";
            DataSet oDT = new DataSet();
            DataTable oTable = new DataTable();
            string oPswDecrip = "";

            try
            {
                oGeneral = new AS_Generales();

                oCS = new Utilerias().getAppSettingsKey("dbalias", _config, "AppSettings");
                ocon = new MSSQLConnection(oCS);
                Procedure = new Utilerias().getAppSettingsKey("SimpleAndFastChekPermission", _config, "StoredProcedures");
                List<Parametros> oParam = new List<Parametros>();
                oParam.Add(new Parametros("@in_agencyid_i", oAgencia, Parametros.SType.Int));
                oParam.Add(new Parametros("@in_user_s", oOperador, Parametros.SType.VarChar));
                oParam.Add(new Parametros("@in_codpais_i", IdPais, Parametros.SType.Int));
                oParam.Add(new Parametros("@in_productid_s", oProducto, Parametros.SType.VarChar));
                bool ob = false;
                try
                {
                    ob = ocon.executeSP(Procedure, oParam, MSSQLConnection.ReturnTypes.Dataset);

                }
                catch (SqlException oExQ)
                {
                    oError = oError + " /  " + oExQ.Message;
                    return false;
                }
                catch (Exception oEx)
                {
                    oError = oError + " /  " + oEx.Message;
                    return false;
                }
                #region resultado nulo
                if (!ob)
                {
                    oError = "Error de ejecución SP al momento de obtener las credenciales del operador";
                    return false;
                }
                #endregion

                oDT = ocon.getDataset();
                oTable = oDT.Tables[0];


                if (oTable.Rows.Count == 0)
                {
                    oError = "No se encuentran operadores con estas credenciales";
                    return false;
                }

                oPswDecrip = new AS_Operadores().mDecriptarPsw(oTable.Rows[0]["password"].ToString(), ref oError);

                //Si es Operador Común, validamos solamente que tenga el mismo password con el de entrada
                if (oTable.Rows[0]["escallcenterope"].ToString().Equals("0") && oPsw.Equals(oPswDecrip))
                {
                    userOK = true;
                }
                //Si es Operador CallCenter, validamos que sean los mismos password y que estén en el mismo grupo
                else if (oTable.Rows[0]["escallcenterope"].ToString().Equals("1") && oPsw.Equals(oPswDecrip) && ((oTable.Rows[0]["codigogrupocall"].ToString().Equals("0") && oTable.Rows[0]["permisocallcenter"].ToString().Equals("True")) || oTable.Rows[0]["mismogrupo"].ToString().Equals("1")))
                {
                    userOK = true;
                }
                else
                    userOK = false;

            }
            catch (Exception oex)
            {

                oError = oError + "/" + oex.Message;
                userOK = false;
            }

            return userOK;
        }

        internal bool ValidaCredencialesTracicional(int remesadorID, string productShortName, ref string oError, IConfiguration _config)
        {
            DataSet oDT = new DataSet();
            DataTable oTable = new DataTable();
            bool ob = false;
            bool resultado = false;
            oCS = new Utilerias().getAppSettingsKey("dbalias", _config, "AppSettings");
            Procedure = new Utilerias().getAppSettingsKey("GetRemesadorConfig", _config, "StoredProcedures");
            ocon = new MSSQLConnection(oCS);
            List<Parametros> oParam = new List<Parametros>();
            oParam.Add(new Parametros("@RemesadorID", remesadorID, Parametros.SType.Int));
            oParam.Add(new Parametros("@ProductShortName", productShortName, Parametros.SType.VarChar));

            try
            {
                ob = ocon.executeSP(Procedure, oParam, MSSQLConnection.ReturnTypes.Dataset);
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
                ocon.closeConnection();
            }

            oDT = ocon.getDataset();

            if (oDT.Tables.Count == 0)
            {
                oError = "La busqueda no obtuvo ningún dato del Remesador";
            }

            oTable = oDT.Tables[0];
            if (oTable.Rows.Count == 0)
            {
                oError = "La busqueda no obtuvo ningún dato del remesador";
            }

            var validaCresd = (from row in oTable.AsEnumerable()
                               select new { validaCredRemesador = row.Field<bool>("ValidaCredencialesTradicional") }).FirstOrDefault();
            resultado = validaCresd.validaCredRemesador;
            if (validaCresd == null)
            {
                oError = "No se encontro la columna ValidaCredencialesTradicional ";

            }
            return resultado;
        }

    }

}
