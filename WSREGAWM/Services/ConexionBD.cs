using GrupoCoen.Corporativo.Libraries.ConexionBD;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using WSREGAWM.Entities;
using WSREGAWM.Helpers;

namespace WSREGAWM.Services
{
    class ConexionBD
    {
        MSSQLConnection oCon;
        string oCS = String.Empty;

        internal WorkFlows obtenerWorkFlow(IConfiguration _config, string operacion, string producto, int remesador, ref string oError)
        {
            DataSet oDT = new DataSet();
            DataTable oTable = new DataTable();
            bool result = false;
            WorkFlows oWF = new WorkFlows();

            try
            {
                oCS = new UtileriasAWM().getAppSettingsKey("dbalias", _config);

                oCon = new MSSQLConnection(oCS);

                List<Parametros> oParam = new List<Parametros>();
                oParam.Add(new Parametros("@OperationID", operacion, Parametros.SType.VarChar));
                oParam.Add(new Parametros("@ProductoID", producto, Parametros.SType.VarChar));
                oParam.Add(new Parametros("@RemesadorID", remesador, Parametros.SType.Int));

                try
                {
                    result = oCon.executeSP("[remesadores].[sp_select_GWMMWorkFlow]", oParam, MSSQLConnection.ReturnTypes.Dataset);
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
                {
                    if (string.IsNullOrEmpty(oError))
                        oError = oCon.getMessage();

                    return new WorkFlows();
                }

                oDT = oCon.getDataset();

                if (oDT.Tables.Count == 0)
                {
                    oError = "La busqueda no obtuvo ningún dato de WorkFlow";
                    throw new Exception();
                }

                oTable = oDT.Tables[0];

                if (oTable.Rows.Count == 0)
                {
                    oError = "La busqueda no obtuvo ningún dato de Workflow";
                    throw new Exception();
                }

                oWF = (from query in oTable.AsEnumerable()
                       select new WorkFlows
                       {
                           NombreWorkFlow = query.Field<string>("NombreWorkFlow"),
                           ConsumeAutorizador = Convert.ToBoolean(query.Field<Boolean>("ConsumeAutorizador")),
                           ConsumeProxy = Convert.ToBoolean(query.Field<Boolean>("ConsumeProxy")),
                           GuardaTransaccion = Convert.ToBoolean(query.Field<Boolean>("GuardaTransaccion"))
                       }).FirstOrDefault();

                var oTable2 = oDT.Tables[1];

                if (oTable2.Rows.Count != 0)
                {
                    oWF.Operaciones = new List<Operacion>();
                    oWF.Operaciones = (from query in oTable2.AsEnumerable()
                                       select new Operacion
                                       {
                                           NombreOperacion = query.Field<string>("NombreOperacion"),
                                           actConsumeAutorizador = Convert.ToBoolean(query.Field<Boolean>("Act_ConsumeAutorizador")),
                                           actConsumeProxy = Convert.ToBoolean(query.Field<Boolean>("Act_ConsumeProxy"))
                                       }).ToList();

                }
            }
            catch (Exception ex)
            {
                oError = ex.Message;
                return new WorkFlows();
            }

            return oWF;
        }

        internal dynamic imprimirBoleta(IConfiguration _config, string MTCN, string fecha, string tipoOperacion,
            string paisValidacion, ref string oError)
        {
            DataSet oDT = new DataSet();
            DataTable oTable = new DataTable();
            WorkFlows oWF = new WorkFlows();
            bool result = false;
            string jSON = "";

            try
            {
                oCS = new UtileriasAWM().getAppSettingsKey("dbalias", _config);

                oCon = new MSSQLConnection(oCS);

                List<Parametros> oParam = new List<Parametros>();
                oParam.Add(new Parametros("@MTCN", MTCN, Parametros.SType.VarChar));
                oParam.Add(new Parametros("@Fecha", Convert.ToDateTime(fecha), Parametros.SType.DateTime));
                oParam.Add(new Parametros("@TipoTransaccion", tipoOperacion, Parametros.SType.VarChar));
                oParam.Add(new Parametros("@PaisValidacion", paisValidacion, Parametros.SType.VarChar));

                try
                {
                    result = oCon.executeSP("[remesadores].[sp_GWMM_ConsumirTracs]", oParam, MSSQLConnection.ReturnTypes.Dataset);
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
                {
                    if (string.IsNullOrEmpty(oError))
                        oError = oCon.getMessage();

                    //return new WorkFlows();
                }

                oDT = oCon.getDataset();

                if (oDT.Tables.Count == 0)
                {
                    oError = "La busqueda no obtuvo ningún dato de Tracs";
                    throw new Exception();
                }

                oTable = oDT.Tables[0];

                if (oTable.Rows.Count == 0)
                {
                    oError = "La busqueda no obtuvo ningún dato de Tracs";
                    return oWF;
                    //throw new Exception();
                }

                var json = (from query in oTable.AsEnumerable()
                            select new
                            {
                                EnvioID = query.Field<int>("EnvioID"),
                                Cuenta = query.Field<string>("Cuenta"),
                                Fecha = query.Field<DateTime>("Fecha"),
                                MTCN = query.Field<double>("MTCN"),
                                Monto = query.Field<decimal>("Monto"),
                                Cargos = query.Field<decimal>("Cargos"),
                                Impuesto = query.Field<decimal>("Impuesto"),
                                CargosxMensaje = query.Field<decimal>("CargosxMensaje"),
                                CargosxEntrega = query.Field<decimal>("CargosxEntrega"),
                                MontoML = query.Field<decimal>("MontoML"),
                                CargosMl = query.Field<decimal>("CargosMl"),
                                ImpuestoML = query.Field<decimal>("ImpuestoML"),
                                CargosxMensajeML = query.Field<decimal>("CargosxMensajeML"),
                                CargosxEntregaML = query.Field<decimal>("CargosxEntregaML"),
                                Tasa = query.Field<decimal>("Tasa"),
                                Agencia = query.Field<string>("Agencia"),
                                Grupo = query.Field<string>("Grupo"),
                                Remitente = query.Field<string>("Remitente"),
                                Beneficiario = query.Field<string>("Beneficiario"),
                                CiudadOrigen = query.Field<string>("CiudadOrigen"),
                                EstadoOrigen = query.Field<string>("EstadoOrigen"),
                                PaisDestino = query.Field<string>("PaisDestino"),
                                EstadoDestino = query.Field<string>("EstadoDestino"),
                                CiudadDestino = query.Field<string>("CiudadDestino"),
                                TipoEnvio = query.Field<Byte>("TipoEnvio"),
                                NIT = query.Field<string>("NIT"),
                                Serie = query.Field<string>("Serie"),
                                NumFact = query.Field<int>("NumFact"),
                                NameFact = query.Field<string>("NameFact"),
                                DirFact = query.Field<string>("DirFact"),
                                Operador = query.Field<string>("Operador"),
                                NumeroOperador = query.Field<Int16>("NumeroOperador"),
                                Terminal = query.Field<string>("Terminal"),
                                WUCard = query.Field<string>("WUCard"),
                                CtaBanco = query.Field<int>("CtaBanco"),
                                FechaCreacion = query.Field<DateTime>("FechaCreacion"),
                                Moneda = query.Field<string>("Moneda"),
                                ModoRegistro = query.Field<string>("ModoRegistro")
                            }).FirstOrDefault();
                if (json.MTCN.Equals(""))
                {
                    return oWF;
                }
                else
                {
                    jSON = Convert.ToString(json);
                }
            }
            catch (Exception ex)
            {
                oError = ex.Message;
                //return new WorkFlows();
            }
            var resultado1 = jSON;
            return resultado1;
        }

        public static String GetDSJSon(System.Data.DataTable tabla_origen, String nombre)
        {
            DataSet dsGenerado = new DataSet("dataSet");
            dsGenerado.Namespace = "NetFrameWork";
            DataTable tabla = new DataTable();
            tabla.TableName = nombre;
            for (int i = 0; i < tabla_origen.Columns.Count; i++)
            {
                DataColumn columna = new DataColumn(tabla_origen.Columns[i].ColumnName, tabla_origen.Columns[i].DataType);
                tabla.Columns.Add(columna);
            }
            dsGenerado.Tables.Add(tabla);
            for (int i = 0; i < tabla_origen.Rows.Count; i++)
            {
                DataRow newRow = tabla.NewRow();
                for (int j = 0; j < tabla_origen.Columns.Count; j++)
                {
                    newRow[tabla_origen.Columns[j].ColumnName] = tabla_origen.Rows[i][j];
                }
                tabla.Rows.Add(newRow);
            }
            dsGenerado.AcceptChanges();
            string json = JsonConvert.SerializeObject(dsGenerado, Formatting.Indented);
            return json;
        }
        public string getUrl(int paisID, ref string oError, IConfiguration _config)
        {
            DataSet oDT = new DataSet();
            DataTable oTable = new DataTable();
            bool executeResult = false;
            string encontrado = "";
            string Procedura = string.Empty;
            string paisURL= String.Empty;
            //
            List<Parametros> iParams = new List<Parametros>();
            Parametros iparam = new Parametros("@paisId", paisID, Parametros.SType.Int);
            iParams.Add(iparam);

            string dbAlias = new UtileriasAWM().getAppSettingsKey("dbalias", _config);
            Procedura = new UtileriasAWM().getAppSettingsKey("getUrlPais", _config, "StoredProcedures");
            MSSQLConnection ocon = new MSSQLConnection(dbAlias);
            try
            {
                executeResult = ocon.executeSP(Procedura, iParams, MSSQLConnection.ReturnTypes.Dataset);
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
 
            foreach (DataRow item in oTable.Rows)
            {
                paisURL = item["Url"].ToString();
            }
            return paisURL;
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

                oCS = new UtileriasAWM().getAppSettingsKey("dbalias", _config);
                Procedure = new UtileriasAWM().getAppSettingsKey("getIdPais", _config, "StoredProcedures");
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

    }
}
