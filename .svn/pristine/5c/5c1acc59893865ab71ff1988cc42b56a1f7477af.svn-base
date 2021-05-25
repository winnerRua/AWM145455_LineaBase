using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GrupoCoen.Corporativo.Libraries.AH2HAS.Clases;
using Microsoft.Win32;
using GrupoCoen.Corporativo.Libraries.ConexionBD;
using System.Data;
using System.Data.SqlClient;

namespace GrupoCoen.Corporativo.Libraries.AH2HAS.Utilidades
{
    public class AS_Generales
    {

        public bool mGetNameUser(string oIdUser, ref string oNombre, ref string oError)
        {
            List<Parametros> oParam = new List<Parametros>();
            string oCT = "";
            DataSet oDT = new DataSet();
            DataTable oTable = new DataTable();
            GrupoCoen.Corporativo.Libraries.ConexionBD.MSSQLConnection ocon;
            bool oResult = false;
            string oCS = "";
            try
            {
                oCS = mCSH2H(ref oCT);
                ocon = new MSSQLConnection(oCS);

                bool ob = false;
                oParam.Add(new Parametros("@userid", oIdUser, Parametros.SType.VarChar));
                try
                {
                    ob = ocon.executeSP("sp_as_get_usuarioName", oParam, GrupoCoen.Corporativo.Libraries.ConexionBD.MSSQLConnection.ReturnTypes.Dataset);
                }
                catch (SqlException oExQ)
                {
                    return false;
                }
                catch (Exception oEx)
                {
                    return false;
                }
                if (!ob)
                {
                    return false; ;
                }

                oResult = true;
                oDT = ocon.getDataset();
                if (oDT.Tables.Count == 0)
                {
                    return false;
                }
                oTable = oDT.Tables[0];
                if (oTable.Rows.Count == 0)
                {
                    return false;
                }
                oNombre = oTable.Rows[0]["Nombre"].ToString();
            }
            catch
            {
                oResult = false;
                oTable = null;
            }

            return oResult;
        }

        public string mCSH2H(ref string oLlave)
        {
            string Key = "";
            try
            {
                Key = System.Configuration.ConfigurationManager.AppSettings["dbalias"];
                RegistryKey registryAccess = Registry.LocalMachine;
                registryAccess = registryAccess.OpenSubKey("SOFTWARE");
                registryAccess = registryAccess.OpenSubKey("_GrupoCoen");
                oLlave = registryAccess.GetValue(Key).ToString();
            }
            catch
            {
                try
                {

                    Key = System.Configuration.ConfigurationManager.AppSettings["dbalias"];
                    RegistryKey registryAccess = Registry.LocalMachine;
                    registryAccess = registryAccess.OpenSubKey("SOFTWARE");
                    registryAccess = registryAccess.OpenSubKey("Wow6432Node");
                    registryAccess = registryAccess.OpenSubKey("_GrupoCoen");
                    oLlave = registryAccess.GetValue(Key).ToString();
                }
                catch { throw new Exception("key no found"); }
                //  throw new Exception("key no found");
            }
            return Key;
        }

        public string mCSSeg(ref string oLlave)
        {
            string Key = "";
            try
            {
                //Key = System.Configuration.ConfigurationManager.AppSettings["dbSegC"];
                Key = System.Configuration.ConfigurationManager.AppSettings["dbalias"];
                RegistryKey registryAccess = Registry.LocalMachine;
                registryAccess = registryAccess.OpenSubKey("SOFTWARE");
                registryAccess = registryAccess.OpenSubKey("_GrupoCoen");
                oLlave = registryAccess.GetValue(Key).ToString();
            }
            catch
            {
                try
                {
                    //Key = System.Configuration.ConfigurationManager.AppSettings["dbSegC"];
                    Key = System.Configuration.ConfigurationManager.AppSettings["dbalias"];
                    RegistryKey registryAccess = Registry.LocalMachine;
                    registryAccess = registryAccess.OpenSubKey("SOFTWARE");
                    registryAccess = registryAccess.OpenSubKey("Wow6432Node");
                    registryAccess = registryAccess.OpenSubKey("_GrupoCoen");
                    oLlave = registryAccess.GetValue(Key).ToString();
                }
                catch { throw new Exception("key no found"); }
                //  throw new Exception("key no found");
            }
            return Key;
        }

        public string mPasskey()
        {
            string Key = "";
            try
            {
                RegistryKey registryAccess = Registry.LocalMachine;
                registryAccess = registryAccess.OpenSubKey("SOFTWARE");
                registryAccess = registryAccess.OpenSubKey("_GrupoCoen");
                registryAccess = registryAccess.OpenSubKey("ACP");
                Key = registryAccess.GetValue("PassKey").ToString();
            }
            catch
            {
                try
                {
                    RegistryKey registryAccess = Registry.LocalMachine;
                    registryAccess = registryAccess.OpenSubKey("SOFTWARE");
                    registryAccess = registryAccess.OpenSubKey("Wow6432Node");
                    registryAccess = registryAccess.OpenSubKey("_GrupoCoen");
                    registryAccess = registryAccess.OpenSubKey("ACP");
                    Key = registryAccess.GetValue("PassKey").ToString();
                }
                catch { throw new Exception("key no found"); }
            }
            return Key;
        }

        public bool mlistaAgencia(string oPais, string oVariable, string oOpcion, ref DataTable oTable)
        {
            List<Parametros> oParam = new List<Parametros>();
            string oCT = "";
            DataSet oDT = new DataSet();
            GrupoCoen.Corporativo.Libraries.ConexionBD.MSSQLConnection ocon;
            bool oResult = false;
            string oCS = "";
            string oConexions = "";
            bool ob;
            try
            {
                oCS = mCSH2H(ref oConexions);
                ocon = new MSSQLConnection(oCS);

                oParam.Add(new Parametros("@pais", oPais, Parametros.SType.Int));
                oParam.Add(new Parametros("@variable", oVariable, Parametros.SType.Int));
                oParam.Add(new Parametros("@opcion", oOpcion, Parametros.SType.Int));

                try
                {
                    ob = ocon.executeSP("sp_as_Grupo_Agencia", oParam, GrupoCoen.Corporativo.Libraries.ConexionBD.MSSQLConnection.ReturnTypes.Dataset);
                }
                catch (SqlException oExQ)
                {
                    return false;
                }
                catch (Exception oEx)
                {
                    return false;
                }
                if (!ob)
                {
                    oTable = null;
                    return false; ;
                }

                oDT = ocon.getDataset();
                oTable = oDT.Tables[0];
                oResult = true;
            }
            catch
            {
                oResult = false;
            }

            return oResult;
        }

        public bool mGrupo(string oAgencia, string oPais, ref DataTable oTable)
        {
            List<Parametros> oParam = new List<Parametros>();
            string oCT = "";
            DataSet oDT = new DataSet();
            GrupoCoen.Corporativo.Libraries.ConexionBD.MSSQLConnection ocon;
            bool oResult = false;
            string oCS = "";
            try
            {
                oCS = mCSH2H(ref oCT);
                ocon = new MSSQLConnection(oCS);

                bool ob = false;
                oParam.Add(new Parametros("@pais", oPais, Parametros.SType.Int));
                oParam.Add(new Parametros("@variable", oAgencia, Parametros.SType.Int));
                oParam.Add(new Parametros("@opcion", "3", Parametros.SType.Int));

                try
                {
                    ob = ocon.executeSP("sp_as_Grupo_Agencia", oParam, GrupoCoen.Corporativo.Libraries.ConexionBD.MSSQLConnection.ReturnTypes.Dataset);
                }
                catch (SqlException oExQ)
                {
                    return false;
                }
                catch (Exception oEx)
                {
                    return false;
                }
                if (!ob)
                {
                    oTable = null;
                    return false; ;
                }

                oResult = true;
                oDT = ocon.getDataset();
                oTable = oDT.Tables[0];
            }
            catch
            {
                oResult = false;
                oTable = null;
            }

            return oResult;
        }

        public bool mlistaPais(string oUsuario, string oOpcion, ref DataTable oTable, ref string oError)
        {
            List<Parametros> oParam = new List<Parametros>();
            DataSet oDT = new DataSet();
            GrupoCoen.Corporativo.Libraries.ConexionBD.MSSQLConnection ocon;
            bool oResult = false;
            string oCS = "";
            string oConexions = "";
            bool ob;
            try
            {
                oCS = mCSH2H(ref oConexions);
                ocon = new MSSQLConnection(oCS);

                oParam.Add(new Parametros("@userid", oUsuario, Parametros.SType.VarChar));
                oParam.Add(new Parametros("@opcion", oOpcion, Parametros.SType.VarChar));
                try
                {
                    ob = ocon.executeSP("_sp_AS_seg_ControlPais", oParam, GrupoCoen.Corporativo.Libraries.ConexionBD.MSSQLConnection.ReturnTypes.Dataset);
                }
                catch (SqlException oExQ)
                {
                    oError = oExQ.Message;
                    return false;
                }
                catch (Exception oEx)
                {
                    oError = oEx.Message;
                    return false;
                }
                if (!ob)
                {
                    oError = "No se obtuvo resultado para esta solicitud.";
                    oTable = null;
                    return false; ;
                }

                oDT = ocon.getDataset();

                if (oDT.Tables.Count == 0)
                {
                    oError = "El usuario no tiene permisos asignados";
                    return false;
                }

                oTable = oDT.Tables[0];

                if (oTable.Rows.Count == 0)
                {
                    oError = "El usuario no tiene permisos asignados";
                    return false;
                }
                oError = "";
                oResult = true;
            }
            catch
            {
                oResult = false;
            }

            return oResult;
        }

        public bool mCatalogoProdMX(ref DataTable oProductos, ref string oError)
        {
            bool ob = false;
            string oCT = "";
            DataSet oDT = new DataSet();
            MSSQLConnection ocon;
            try
            {
                string oCS = mCSH2H(ref oCT);
                ocon = new MSSQLConnection(oCS);

                List<Parametros> oParam = new List<Parametros>();

                #region insertar
                try
                {
                    ob = ocon.executeSP("sp_h2h_get_productosmx_mt", oParam, GrupoCoen.Corporativo.Libraries.ConexionBD.MSSQLConnection.ReturnTypes.Dataset);
                }
                catch (SqlException oExQ)
                {
                    oError = "Error en ejecucion de SP:  " + oExQ.Message;
                    ob = false;
                }
                catch (Exception oEx)
                {
                    ob = false;
                    oError = "Error en ejecucion de SP:  " + oEx.Message;
                }
                #endregion

                oDT = ocon.getDataset();
                oProductos = oDT.Tables[0];
                if (oProductos.Rows.Count == 0)
                {
                    oError = " Error: Hubo problemas a la hora de cargar el producto ";
                    return false;
                }
            }
            catch (Exception oex)
            {
                ob = false;
                oError = "Error: ";
                oError = oError + " / " + oex.Message;
            }

            return ob;
        }

        #region Cashapp
        public string mDBSyR(ref string oLlave)
        {
            string Key = "";
            try
            {
                Key = System.Configuration.ConfigurationManager.AppSettings["db_SyR"];
                RegistryKey registryAccess = Registry.LocalMachine;
                registryAccess = registryAccess.OpenSubKey("SOFTWARE");
                registryAccess = registryAccess.OpenSubKey("_GrupoCoen");
                oLlave = registryAccess.GetValue(Key).ToString();
            }
            catch
            {
                try
                {

                    Key = System.Configuration.ConfigurationManager.AppSettings["db_SyR"];
                    RegistryKey registryAccess = Registry.LocalMachine;
                    registryAccess = registryAccess.OpenSubKey("SOFTWARE");
                    registryAccess = registryAccess.OpenSubKey("Wow6432Node");
                    registryAccess = registryAccess.OpenSubKey("_GrupoCoen");
                    oLlave = registryAccess.GetValue(Key).ToString();
                }
                catch { throw new Exception("key no found"); }
                //  throw new Exception("key no found");
            }
            return Key;
        }
        #endregion
    }
}
