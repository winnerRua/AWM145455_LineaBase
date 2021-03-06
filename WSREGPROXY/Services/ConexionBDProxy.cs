using GrupoCoen.Corporativo.Libraries.ConexionBD;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using WSREGPROXY.Helpers;

namespace WSREGPROXY.Services
{
    public class ConexionBDProxy
    {
        MSSQLConnection ocon;
        string oCS = string.Empty;
        string Procedure = string.Empty;
        internal DataTable getRemesadorConfig(int remesadorID, string productShortName, ref string oError, IConfiguration _config)
        {
            DataSet oDT = new DataSet();
            DataTable oTable = new DataTable();
            bool ob = false;

            oCS = new UtileriasProxy().getAppSettingsKey("dbalias", _config, "AppSettings");
            Procedure = new UtileriasProxy().getAppSettingsKey("GetRemesadorConfig", _config, "StoredProcedures");
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

            return oTable;
        }

        public object obtenerWorkFlow(IConfiguration config, string v1, string v2, ref string oError)
        {
            throw new NotImplementedException();
        }

        //parametros del remesador
        internal DataTable getParamRemesador(int remesadorID, string operationID,string productShortName, ref DataTable TableParam, ref DataTable TableDefaultValues, ref DataTable TableCatalogos, ref string oError, IConfiguration _config)
        {
            DataSet oDT = new DataSet();
            DataTable oTable = new DataTable();
            bool ob = false;

            try
            {
                string oCS = new UtileriasProxy().getAppSettingsKey("dbalias", _config, "AppSettings");
                ocon = new MSSQLConnection(oCS);
                Procedure = new UtileriasProxy().getAppSettingsKey("getRemesadorParametros", _config, "StoredProcedures");
                List<Parametros> oParam = new List<Parametros>();
                //oParam.Add(new Parametros("@ConfigRemesadorID", remesadorConfigID, Parametros.SType.Int));
                oParam.Add(new Parametros("@remesadorID", remesadorID, Parametros.SType.Int));
                oParam.Add(new Parametros("@OperationID", operationID, Parametros.SType.VarChar));
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
                    oError = "La busqueda no obtuvo ningún parámetro del remesador";
                }

                oTable = oDT.Tables[0];

                if (oTable.Rows.Count == 0)
                {
                    oError = "La busqueda no obtuvo ningún parámetro del remesador";
                }
                //regresa el select de la localizacoin de los nodos de la trama del partner y del remesador
                if (oDT.Tables[1].Rows.Count == 0)
                {
                    oError = "La busqueda no obtuvo la locaclización de los parametros";
                }

                TableParam = oDT.Tables[1];
                //regresa el select de valores default
                if (oDT.Tables[2].Rows.Count > 0)
                {
                    TableDefaultValues = oDT.Tables[2];
                }
                else
                {
                    TableDefaultValues = new DataTable();
                }

                //regresa el select de valores de catalogos
                if (oDT.Tables[3].Rows.Count > 0)
                {
                    TableCatalogos = oDT.Tables[3];
                }
                else
                {
                    TableCatalogos = new DataTable();
                }

                return oTable;
            }
            catch (Exception oex)
            {
                oError = oError + " / " + oex.Message;
            }
            return new DataTable();

        }
        internal DataTable getNameSpaceRemesador(int RemesadorID, ref string oError, IConfiguration _config)
        {
            DataSet oDT = new DataSet();
            DataTable oTable = new DataTable();
            bool ob = false;

            try
            {
                string oCS = new UtileriasProxy().getAppSettingsKey("dbalias", _config, "AppSettings");
                ocon = new MSSQLConnection(oCS);
                Procedure = new UtileriasProxy().getAppSettingsKey("getNameSpaces", _config, "StoredProcedures");
                List<Parametros> oParam = new List<Parametros>();
                oParam.Add(new Parametros("@RemesadorID", RemesadorID, Parametros.SType.Int));

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
                    oError = "La busqueda no obtuvo ningún NameSpace del Remesador";
                }

                oTable = oDT.Tables[0];

                if (oTable.Rows.Count == 0)
                {
                    oError = "La busqueda no obtuvo ningún NameSpace del Remesador";
                }

                return oTable;
            }
            catch (Exception oex)
            {
                oError = oError + " / " + oex.Message;
            }
            return new DataTable();

        }
        internal DataTable getRemesadorParamNameSpace(int remesadorID, ref string oError, IConfiguration _config)
        {
            DataSet oDT = new DataSet();
            DataTable oTable = new DataTable();
            bool ob = false;

            try
            {
                string oCS = new UtileriasProxy().getAppSettingsKey("dbalias", _config, "AppSettings");
                ocon = new MSSQLConnection(oCS);
                Procedure = new UtileriasProxy().getAppSettingsKey("getParamNameSpaces", _config, "StoredProcedures");

                List<Parametros> oParam = new List<Parametros>();
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
                    oError = "La busqueda no obtuvo ningún NameSpace de parametro";
                }

                oTable = oDT.Tables[0];

                if (oTable.Rows.Count == 0)
                {
                    oError = "La busqueda no obtuvo ningún NameSpace de parametro";
                }

                return oTable;
            }
            catch (Exception oex)
            {
                oError = oError + " / " + oex.Message;
            }
            return new DataTable();

        }
        internal DataTable getRemesadorHeaderProperties(int remesadorID, string countryCode, ref string oError, IConfiguration _config)
        {

            DataSet oDT = new DataSet();
            DataTable oTable = new DataTable();
            bool ob = false;

            try
            {
                string oCS = new UtileriasProxy().getAppSettingsKey("dbalias", _config, "AppSettings");
                ocon = new MSSQLConnection(oCS);
                Procedure = new UtileriasProxy().getAppSettingsKey("getHeaderProperties", _config, "StoredProcedures");

                List<Parametros> oParam = new List<Parametros>();
                oParam.Add(new Parametros("@RemesadorID", remesadorID, Parametros.SType.Int));
                oParam.Add(new Parametros("@CodigoPais", countryCode, Parametros.SType.VarChar));

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
                    oError = "La busqueda no obtuvo ningún HeaderProperty del remesador";
                }

                oTable = oDT.Tables[0];
                if (oTable.Rows.Count == 0)
                {
                    oError = "La busqueda no obtuvo ningún HeaderProperty del remesador";
                }

                return oTable;
            }
            catch (Exception oex)
            {
                oError = oError + " / " + oex.Message;
            }
            return new DataTable();


        }
        internal DataTable getRemesadorCertificadoInfo(int remesadorID, ref string oError, IConfiguration _config)
        {

            DataSet oDT = new DataSet();
            DataTable oTable = new DataTable();
            bool ob = false;

            try
            {
                string oCS = new UtileriasProxy().getAppSettingsKey("dbalias", _config, "AppSettings");
                ocon = new MSSQLConnection(oCS);
                Procedure = new UtileriasProxy().getAppSettingsKey("getCertificadoRemesador", _config, "StoredProcedures");

                List<Parametros> oParam = new List<Parametros>();
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
                    oError = "La busqueda no obtuvo ningún HeaderProperty de parametro";
                }

                oTable = oDT.Tables[0];

                if (oTable.Rows.Count == 0)
                {
                    oError = "La busqueda no obtuvo ningún HeaderProperty de parametro";
                }

                return oTable;
            }
            catch (Exception oex)
            {
                oError = oError + " / " + oex.Message;
            }
            return new DataTable();
        }
        internal DataTable getInterfazOperacionRemesador(int remesadorID, string operationID, string productShortName ,ref string oError, IConfiguration _config)
        {
            DataSet oDT = new DataSet();
            DataTable oTable = new DataTable();
            bool ob = false;

            try
            {
                string oCS = new UtileriasProxy().getAppSettingsKey("dbalias", _config, "AppSettings");
                ocon = new MSSQLConnection(oCS);
                Procedure = new UtileriasProxy().getAppSettingsKey("getInterfazOperacionRemesador", _config, "StoredProcedures");
                List<Parametros> oParam = new List<Parametros>();
                oParam.Add(new Parametros("@remesadorID", remesadorID, Parametros.SType.Int));
                oParam.Add(new Parametros("@NomOperacion", operationID, Parametros.SType.VarChar));
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
                    oError = "La busqueda no obtuvo informacion de la interfaz del remesador";
                }

                oTable = oDT.Tables[0];

                if (oTable.Rows.Count == 0)
                {
                    oError = "La busqueda no obtuvo informacion de la interfaz del remesador";
                }
                //regresa el select de la localizacoin de los nodos de la trama del partner y del remesador


                return oTable;
            }
            catch (Exception oex)
            {
                oError = oError + " / " + oex.Message;
            }
            return new DataTable();

        }
        internal DataTable GetPartnerUsuarioID(IConfiguration _config,int agencyID, string agencyCode, ref string oError)
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

                oCS = new UtileriasProxy().getAppSettingsKey("dbalias", _config, "AppSettings");
                Procedure = new UtileriasProxy().getAppSettingsKey("GetPartnerUsuario", _config, "StoredProcedures"); ;
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
                return oTable;
            }
            catch (Exception oex)
            {
                oError = oError + " / " + oex.Message;
                return new DataTable();
            }
        }


    }
}
