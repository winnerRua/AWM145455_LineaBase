using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GrupoCoen.Corporativo.Libraries.AH2HAS.Clases;
using GrupoCoen.Corporativo.Libraries.ConexionBD;
using GrupoCoen.Corporativo.Libraries.AH2HAS.Utilidades;
using System.Data.SqlClient;
using System.Data;

namespace GrupoCoen.Corporativo.Libraries.AH2HAS.Operaciones.Operadores
{
    public class AS_Operadores
    {

        GrupoCoen.Corporativo.Libraries.ConexionBD.MSSQLConnection ocon;
        public GrupoCoen.Corporativo.Libraries.AH2HAS.Utilidades.AS_Generales oGeneral;
        public string mEncriptarPsw(string oPs, ref string oError)
        {
            string oRespuesta = "";

            try
            {
                //obtiene la llave
                GrupoCoen.Corporativo.ProjectCenter.Libraries.CriptoEngine.BFEngine oDecripKey = new GrupoCoen.Corporativo.ProjectCenter.Libraries.CriptoEngine.BFEngine();
                string oKey = oDecripKey.Process(oGeneral.mPasskey(), GrupoCoen.Corporativo.ProjectCenter.Libraries.CriptoEngine.BFEngine.AccionCifrado.Decrypt);

                //se encripta el psw
                GrupoCoen.Corporativo.ProjectCenter.Libraries.CriptoEngine.BFEngine oEncripTrama = new GrupoCoen.Corporativo.ProjectCenter.Libraries.CriptoEngine.BFEngine(oKey);
                oRespuesta = oEncripTrama.Process(oPs, GrupoCoen.Corporativo.ProjectCenter.Libraries.CriptoEngine.BFEngine.AccionCifrado.Encrypt);

            }
            catch (Exception ex)
            {
                oError = oError + " ErrorEncript: " + ex.Message;
                oRespuesta = "";
            }

            return oRespuesta;
        }

        public string mDecriptarPsw(string oPs, ref string oError)
        {
            string oRespuesta = "";

            try
            {
                //obtiene la llave
                GrupoCoen.Corporativo.ProjectCenter.Libraries.CriptoEngine.BFEngine oDecripKey = new GrupoCoen.Corporativo.ProjectCenter.Libraries.CriptoEngine.BFEngine();
                string oKey = oDecripKey.Process(new AS_Generales().mPasskey(), GrupoCoen.Corporativo.ProjectCenter.Libraries.CriptoEngine.BFEngine.AccionCifrado.Decrypt);

                //se encripta el psw
                GrupoCoen.Corporativo.ProjectCenter.Libraries.CriptoEngine.BFEngine oEncripTrama = new GrupoCoen.Corporativo.ProjectCenter.Libraries.CriptoEngine.BFEngine(oKey);
                oRespuesta = oEncripTrama.Process(oPs, GrupoCoen.Corporativo.ProjectCenter.Libraries.CriptoEngine.BFEngine.AccionCifrado.Decrypt);

            }
            catch (Exception ex)
            {
                oError = oError + " ErrorEncript: " + ex.Message;
                oRespuesta = "";
            }

            return oRespuesta;
        }

        //public bool mTipoOperador(ref DataTable oTabla)
        //{
        //    bool oResult = false;

        //    List<Parametros> oParam = new List<Parametros>();
        //    string oCT = "";
        //    DataSet oDT = new DataSet();
        //    try
        //    {
        //        oGeneral = new AS_Generales();
        //        string oCS = oGeneral.mCSH2H(ref oCT);
        //        ocon = new MSSQLConnection(oCS);

        //        oParam.Add(new Parametros("@pais", "0", Parametros.SType.Int));

        //        #region insert
        //        bool ob = false;
        //        try
        //        {
        //            ob = ocon.executeSP("sp_as_tipo_operador", oParam, MSSQLConnection.ReturnTypes.Dataset);
        //        }
        //        catch (SqlException oExQ)
        //        {
        //            return false;

        //        }
        //        catch (Exception oEx)
        //        {
        //            return false;
        //        }
        //        #endregion

        //        #region resultado nulo
        //        if (!ob)
        //        {
        //            return false;
        //        }
        //        #endregion

        //        oDT = ocon.getDataset();

        //        oTabla = oDT.Tables[0];
        //        oResult = true;
        //    }
        //    catch (Exception oex)
        //    {
        //        return false;
        //    }
        //    return oResult;
        //}

        //public bool mValidarOp(string oOperador, string oPsw, string oAgencia, ref string oError)
        //{
        //    bool userOK = false;
        //    string oCT = "";
        //    DataSet oDT = new DataSet();
        //    DataTable oTable = new DataTable();
        //    string oPswDB = "";
        //    string oPswEncrip = "";

        //    try
        //    {
        //        oGeneral = new AS_Generales();
        //        string oCS = oGeneral.mCSH2H(ref oCT);
        //        ocon = new MSSQLConnection(oCS);

        //        List<Parametros> oParam = new List<Parametros>();
        //        oParam.Add(new Parametros("@operador", oOperador, Parametros.SType.NVarChar));
        //        oParam.Add(new Parametros("@agencia", oAgencia, Parametros.SType.Int));
        //        bool ob = false;
        //        try
        //        {
        //            ob = ocon.executeSP("sp_as_Auth_Operador1", oParam, MSSQLConnection.ReturnTypes.Dataset);

        //        }
        //        catch (SqlException oExQ)
        //        {

        //            oError = oError + " /  " + oExQ.Message;
        //            return false;

        //        }
        //        catch (Exception oEx)
        //        {

        //            oError = oError + " /  " + oEx.Message;
        //            return false;
        //        }
        //        #region resultado nulo
        //        if (!ob)
        //        {
        //            oError = "Error de ejecución SP al momento de obtener las credenciales del operador";
        //            return false;
        //        }
        //        #endregion

        //        oDT = ocon.getDataset();
        //        oTable = oDT.Tables[0];


        //        if (oTable.Rows.Count == 0)
        //        {
        //            oError = "No se encuentran operadores con estas credenciales";
        //            return false;
        //        }

        //        oPswDB = oTable.Rows[0]["psw"].ToString();

        //        oPswEncrip = mDecriptarPsw(oPswDB, ref oError);

        //        if (oPsw.Equals(oPswEncrip))
        //        {
        //            userOK = true;
        //        }
        //        else
        //            userOK = false;

        //    }
        //    catch (Exception oex)
        //    {

        //        oError = oError + "/" + oex.Message;
        //        userOK = false;
        //    }

        //    return userOK;
        //}

     

        







    }
}
