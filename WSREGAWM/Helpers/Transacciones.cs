using System;
using System.Collections.Generic;
using System.Text;
using GrupoCoen.Corporativo.Libraries.ConexionBD;
using System.Globalization;
using WSREGPROXY.Helpers;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using WSREGAWM;

namespace WSREGAWM.Helpers
{
    public class Transacciones
    {
        private string _errorMessage = "";
        UtileriasProxy utiProxy = new UtileriasProxy();
        internal bool storeSendMoney(int pais, Dictionary<dynamic, dynamic> ENVIO, IConfiguration _config, bool guardaTransaccion, string key)
        {
            List<Parametros> iParams = new List<Parametros>();
            bool opResult = false;
            UtileriasAWM miUtilerias = new UtileriasAWM();
            MSSQLConnection objDb = null;
            try
            {
                ///***********************************************************************************************************/
                Parametros iparam = new Parametros("@PaisValidacion", ENVIO["PaisValidacion"], Parametros.SType.VarChar);
                iParams.Add(iparam);

                //iparam = new Parametros("@TipoTransaccion", ENVIO["tipoTransaccion"], Parametros.SType.VarChar);
                //iParams.Add(iparam);

                iparam = new Parametros("@GuardaTransaccion", Convert.ToInt32(guardaTransaccion), Parametros.SType.VarChar);
                iParams.Add(iparam);

                iparam = new Parametros("@inTipoOperacion", "ENVIO", Parametros.SType.VarChar);
                iParams.Add(iparam);

                iparam = new Parametros("@inIdTerminal", ENVIO["IdTerminal"], Parametros.SType.VarChar);
                iParams.Add(iparam);

                iparam = new Parametros("@inPaisId", int.Parse(ENVIO["PaisID"]), Parametros.SType.Int);
                iParams.Add(iparam);

                iparam = new Parametros("@inNumOperador", int.Parse(ENVIO["NumeroOperador"]), Parametros.SType.SmallInt);
                iParams.Add(iparam);

                iparam = new Parametros("@inAgenciaId", int.Parse(ENVIO["AgenciaID"]), Parametros.SType.Int);
                iParams.Add(iparam);

                iparam = new Parametros("@inMTCN", Convert.ToDecimal(ENVIO["MTCN"]), Parametros.SType.Decimal);
                iParams.Add(iparam);

                iparam = new Parametros("@inPaisRemitente", ENVIO["PaisRem"], Parametros.SType.VarChar);
                iParams.Add(iparam);

                iparam = new Parametros("@inEstadoRemitente", ENVIO["EstadoRem"], Parametros.SType.VarChar);
                iParams.Add(iparam);

                iparam = new Parametros("@inCiudadRemitente", ENVIO["CiudadRem"], Parametros.SType.VarChar);
                iParams.Add(iparam);

                iparam = new Parametros("@inPrimerNombreRem", ENVIO["PrimerNombreRem"], Parametros.SType.VarChar);
                iParams.Add(iparam);

                ///***********************************************************************************************************/

                iparam = new Parametros("@inPrimerApellidoRem", ENVIO["PrimerApellidoRem"], Parametros.SType.VarChar);
                iParams.Add(iparam);

                iparam = new Parametros("@inSegundoNombreRem", ENVIO["SegundoNombreRem"], Parametros.SType.VarChar);
                iParams.Add(iparam);

                iparam = new Parametros("@inSegundoApellidoRem", ENVIO["SegundoApellidoRem"], Parametros.SType.VarChar);
                iParams.Add(iparam);

                iparam = new Parametros("@inApellidoCasadaRem", ENVIO["ApellidoCasadaRem"], Parametros.SType.VarChar);
                iParams.Add(iparam);

                iparam = new Parametros("@inRemitente", ENVIO["Remitente"], Parametros.SType.VarChar);
                iParams.Add(iparam);

                iparam = new Parametros("@inPaisBeneficiario", ENVIO["PaisBen"], Parametros.SType.VarChar);
                iParams.Add(iparam);

                iparam = new Parametros("@inEstadoBeneficiario", ENVIO["EstadoBen"], Parametros.SType.VarChar);
                iParams.Add(iparam);

                iparam = new Parametros("@inCiudadBeneficiario", ENVIO["CiudadBen"], Parametros.SType.VarChar);
                iParams.Add(iparam);

                iparam = new Parametros("@inPrimerNombreBen", ENVIO["PrimerNombreBen"], Parametros.SType.VarChar);
                iParams.Add(iparam);

                iparam = new Parametros("@inPrimerApellidoBen", ENVIO["PrimerApellidoBen"], Parametros.SType.VarChar);
                iParams.Add(iparam);

                ///***********************************************************************************************************/

                iparam = new Parametros("@inSegundoNombreBen", ENVIO["SegundoNombreBen"], Parametros.SType.VarChar);
                iParams.Add(iparam);

                iparam = new Parametros("@inSegundoApellidoBen", ENVIO["SegundoApellidoBen"], Parametros.SType.VarChar);
                iParams.Add(iparam);

                iparam = new Parametros("@inApellidoCasadaBen", ENVIO["ApellidoCasadaBen"], Parametros.SType.VarChar);
                iParams.Add(iparam);

                iparam = new Parametros("@inBeneficiario", ENVIO["Beneficiario"], Parametros.SType.VarChar);
                iParams.Add(iparam);

                iparam = new Parametros("@inTipoDocumento", ENVIO["TipoDocumento"], Parametros.SType.VarChar);
                iParams.Add(iparam);

                iparam = new Parametros("@inNumeroDocumento", ENVIO["NumeroDocumento"], Parametros.SType.VarChar);
                iParams.Add(iparam);

                iparam = new Parametros("@inEmitidioPor", ENVIO["EmitidoPor"], Parametros.SType.VarChar);
                iParams.Add(iparam);

                iparam = new Parametros("@inLugarEmision", ENVIO["LugarEmision"], Parametros.SType.VarChar);
                iParams.Add(iparam);

                //DateTime.ParseExact(miUtilerias.getPropertyValue("send-money-store-request.sender.compliance_details.date_of_birth", csObject), "ddMMyyyy", CultureInfo.InvariantCulture),
                iparam = new Parametros("@inFechaEmision", ENVIO["FechaEmision"], Parametros.SType.DateTime);
                iParams.Add(iparam);

                iparam = new Parametros("@inFechaExpiracion", ENVIO["FechaExpiracion"], Parametros.SType.DateTime);
                iParams.Add(iparam);

                ///***********************************************************************************************************/

                iparam = new Parametros("@inNacionalidad", ENVIO["Nacionalidad"], Parametros.SType.VarChar);
                iParams.Add(iparam);

                iparam = new Parametros("@inDireccion1", ENVIO["DirLinea1"], Parametros.SType.VarChar);
                iParams.Add(iparam);

                iparam = new Parametros("@inDireccion2", ENVIO["DirLinea2"], Parametros.SType.VarChar);
                iParams.Add(iparam);

                iparam = new Parametros("@inIndicaciones", ENVIO["Indicaciones"], Parametros.SType.VarChar);
                iParams.Add(iparam);

                iparam = new Parametros("@inSexo", ENVIO["Sexo"], Parametros.SType.Char);
                iParams.Add(iparam);

                iparam = new Parametros("@inEstadoCivil", ENVIO["EstadoCivil"], Parametros.SType.Char);
                iParams.Add(iparam);

                iparam = new Parametros("@inFechaNacimiento", ENVIO["FechaNacimiento"], Parametros.SType.DateTime);
                iParams.Add(iparam);

                iparam = new Parametros("@inProfesion", ENVIO["Profesion"], Parametros.SType.VarChar);
                iParams.Add(iparam);

                iparam = new Parametros("@inLugarTrabajo", ENVIO["LugarTrabajo"], Parametros.SType.VarChar);
                iParams.Add(iparam);

                iparam = new Parametros("@inTipoTelefono", ENVIO["TipoTelefono"], Parametros.SType.VarChar);
                iParams.Add(iparam);

                ///***********************************************************************************************************/

                iparam = new Parametros("@inNumeroTelefono", ENVIO["NumeroTelefono"], Parametros.SType.VarChar);
                iParams.Add(iparam);

                iparam = new Parametros("@inDato1", ENVIO["Dato1"], Parametros.SType.VarChar);
                iParams.Add(iparam);

                iparam = new Parametros("@inDato2", ENVIO["Dato2"], Parametros.SType.VarChar);
                iParams.Add(iparam);

                iparam = new Parametros("@inClienteId", ENVIO["ClienteID"], Parametros.SType.Int);
                iParams.Add(iparam);

                iparam = new Parametros("@inCtaAgencia", ENVIO["CtaAgencia"], Parametros.SType.VarChar);
                iParams.Add(iparam);

                iparam = new Parametros("@inCtaBanco", ENVIO["CtaBanco"], Parametros.SType.Int);
                iParams.Add(iparam);

                iparam = new Parametros("@inMonto", Convert.ToDecimal(ENVIO["Monto"]), Parametros.SType.Decimal);
                iParams.Add(iparam);

                iparam = new Parametros("@inMontoML", ENVIO["MontoML"], Parametros.SType.Decimal);
                iParams.Add(iparam);

                iparam = new Parametros("@inTasa", ENVIO["Tasa"], Parametros.SType.Decimal);
                iParams.Add(iparam);

                iparam = new Parametros("@inCargo", Convert.ToDecimal(ENVIO["Cargos"]), Parametros.SType.Decimal);
                iParams.Add(iparam);

                ///***********************************************************************************************************/

                iparam = new Parametros("@inCargosML", ENVIO["CargosML"], Parametros.SType.Decimal);
                iParams.Add(iparam);

                iparam = new Parametros("@inCargosxEntrega", ENVIO["CargosxEntrega"], Parametros.SType.Decimal);
                iParams.Add(iparam);

                iparam = new Parametros("@inCargosxEntregaML", ENVIO["CargosxEntregaML"], Parametros.SType.Decimal);
                iParams.Add(iparam);

                iparam = new Parametros("@inCargosxMensaje", Convert.ToDecimal(ENVIO["CargosxMensaje"]), Parametros.SType.Decimal);
                iParams.Add(iparam);

                iparam = new Parametros("@inCargosxMensajeML", ENVIO["CargosxMensajeML"], Parametros.SType.Decimal);
                iParams.Add(iparam);

                iparam = new Parametros("@inImpuesto", ENVIO["Impuesto"], Parametros.SType.Decimal);
                iParams.Add(iparam);

                iparam = new Parametros("@inImpuestoML", ENVIO["ImpuestoML"], Parametros.SType.Decimal);
                iParams.Add(iparam);

                iparam = new Parametros("@inWUCard", ENVIO["WUCard"], Parametros.SType.VarChar);
                iParams.Add(iparam);

                iparam = new Parametros("@inNumFact", int.Parse(ENVIO["NumFact"]), Parametros.SType.Int);
                iParams.Add(iparam);

                iparam = new Parametros("@inNameFact", ENVIO["NameFact"], Parametros.SType.VarChar);
                iParams.Add(iparam);

                ///***********************************************************************************************************/

                iparam = new Parametros("@inDirFact", ENVIO["DirFact"], Parametros.SType.VarChar);
                iParams.Add(iparam);

                iparam = new Parametros("@inNIT", ENVIO["NIT"], Parametros.SType.VarChar);
                iParams.Add(iparam);

                iparam = new Parametros("@inSerie", ENVIO["Serie"], Parametros.SType.VarChar);
                iParams.Add(iparam);

                iparam = new Parametros("@inTipoEnvio", ENVIO["TipoEnvio"], Parametros.SType.TinyInt);
                iParams.Add(iparam);

                iparam = new Parametros("@inEstatusAct", ENVIO["EstatusAct"], Parametros.SType.Char);
                iParams.Add(iparam);

                iparam = new Parametros("@inEstatusSet", ENVIO["EstatusSet"], Parametros.SType.Char);
                iParams.Add(iparam);

                iparam = new Parametros("@inUsuariId", ENVIO["UsuarioID"], Parametros.SType.Int);
                iParams.Add(iparam);

                iparam = new Parametros("@inFechaEnvio", ENVIO["FechaEnvio"], Parametros.SType.DateTime);
                iParams.Add(iparam);

                iparam = new Parametros("@inFecha", ENVIO["Fecha"], Parametros.SType.DateTime);
                iParams.Add(iparam);

                iparam = new Parametros("@inFechaCreacion", ENVIO["FechaCreacion"], Parametros.SType.DateTime);
                iParams.Add(iparam);

                /***********************************************************************************************************/
                /***********************************************************************************************************/

                var oCS = new UtileriasAWM().getAppSettingsKey("dbalias", _config);
                objDb = new MSSQLConnection(oCS);
                if (objDb.executeSP("remesadores.sp_GWMM_GuardarTransacciones", iParams, MSSQLConnection.ReturnTypes.Dataset))
                {
                    utiProxy.SaveLog(Convert.ToString(objDb), "1", key, 1, _config);
                    opResult = true;
                }
                else
                {
                    _errorMessage = objDb.getMessage();
                }
            }
            catch (Exception ex)
            {
                _errorMessage = ex.Message;
                opResult = true;
            }
            finally
            {
                if (objDb != null) objDb.closeConnection();
            }
            return opResult;
        }

        internal bool storePayMoney(int pais, Dictionary<dynamic, dynamic> PAGO, IConfiguration _config, bool guardaTransaccion, string key)
        {
            bool opResult = false;
            List<Parametros> iParams = new List<Parametros>();
            UtileriasAWM miUtilerias = new UtileriasAWM();
            MSSQLConnection objDb = null;
            try
            {
                /***********************************************************************************************************/
                //iparam = new Parametros("@TipoTransaccion", ENVIO["tipoTransaccion"], Parametros.SType.VarChar);
                //iParams.Add(iparam);

                Parametros iparam = new Parametros("@PaisValidacion", PAGO["PaisValidacion"], Parametros.SType.VarChar);
                iParams.Add(iparam);

                //iparam = new Parametros("@TipoTransaccion", ENVIO["tipoTransaccion"], Parametros.SType.VarChar);
                //iParams.Add(iparam);

                iparam = new Parametros("@GuardaTransaccion", Convert.ToInt32(guardaTransaccion), Parametros.SType.VarChar);
                iParams.Add(iparam);


                iparam = new Parametros("@inTipoOperacion", "ENVIO", Parametros.SType.VarChar);
                iParams.Add(iparam);

                iparam = new Parametros("@inIdTerminal", PAGO["IdTerminal"], Parametros.SType.VarChar);
                iParams.Add(iparam);

                iparam = new Parametros("@inPaisId", int.Parse(PAGO["PaisID"]), Parametros.SType.Int);
                iParams.Add(iparam);

                iparam = new Parametros("@inNumOperador", int.Parse(PAGO["NumeroOperador"]), Parametros.SType.SmallInt);
                iParams.Add(iparam);

                iparam = new Parametros("@inAgenciaId", int.Parse(PAGO["AgenciaID"]), Parametros.SType.Int);
                iParams.Add(iparam);

                iparam = new Parametros("@inMTCN", Convert.ToDecimal(PAGO["MTCN"]), Parametros.SType.Decimal);
                iParams.Add(iparam);

                iparam = new Parametros("@inPaisRemitente", PAGO["PaisRem"], Parametros.SType.VarChar);
                iParams.Add(iparam);

                iparam = new Parametros("@inEstadoRemitente", PAGO["EstadoRem"], Parametros.SType.VarChar);
                iParams.Add(iparam);

                iparam = new Parametros("@inCiudadRemitente", PAGO["CiudadRem"], Parametros.SType.VarChar);
                iParams.Add(iparam);

                iparam = new Parametros("@inPrimerNombreRem", PAGO["PrimerNombreRem"], Parametros.SType.VarChar);
                iParams.Add(iparam);

                ///***********************************************************************************************************/

                iparam = new Parametros("@inPrimerApellidoRem", PAGO["PrimerApellidoRem"], Parametros.SType.VarChar);
                iParams.Add(iparam);

                iparam = new Parametros("@inSegundoNombreRem", PAGO["SegundoNombreRem"], Parametros.SType.VarChar);
                iParams.Add(iparam);

                iparam = new Parametros("@inSegundoApellidoRem", PAGO["SegundoApellidoRem"], Parametros.SType.VarChar);
                iParams.Add(iparam);

                iparam = new Parametros("@inApellidoCasadaRem", PAGO["ApellidoCasadaRem"], Parametros.SType.VarChar);
                iParams.Add(iparam);

                iparam = new Parametros("@inRemitente", PAGO["Remitente"], Parametros.SType.VarChar);
                iParams.Add(iparam);

                iparam = new Parametros("@inPaisBeneficiario", PAGO["PaisBen"], Parametros.SType.VarChar);
                iParams.Add(iparam);

                iparam = new Parametros("@inEstadoBeneficiario", PAGO["EstadoBen"], Parametros.SType.VarChar);
                iParams.Add(iparam);

                iparam = new Parametros("@inCiudadBeneficiario", PAGO["CiudadBen"], Parametros.SType.VarChar);
                iParams.Add(iparam);

                iparam = new Parametros("@inPrimerNombreBen", PAGO["PrimerNombreBen"], Parametros.SType.VarChar);
                iParams.Add(iparam);

                iparam = new Parametros("@inPrimerApellidoBen", PAGO["PrimerApellidoBen"], Parametros.SType.VarChar);
                iParams.Add(iparam);

                ///***********************************************************************************************************/

                iparam = new Parametros("@inSegundoNombreBen", PAGO["SegundoNombreBen"], Parametros.SType.VarChar);
                iParams.Add(iparam);

                iparam = new Parametros("@inSegundoApellidoBen", PAGO["SegundoApellidoBen"], Parametros.SType.VarChar);
                iParams.Add(iparam);

                iparam = new Parametros("@inApellidoCasadaBen", PAGO["ApellidoCasadaBen"], Parametros.SType.VarChar);
                iParams.Add(iparam);

                iparam = new Parametros("@inBeneficiario", PAGO["Beneficiario"], Parametros.SType.VarChar);
                iParams.Add(iparam);

                iparam = new Parametros("@inTipoDocumento", PAGO["TipoDocumento"], Parametros.SType.VarChar);
                iParams.Add(iparam);

                iparam = new Parametros("@inNumeroDocumento", PAGO["NumeroDocumento"], Parametros.SType.VarChar);
                iParams.Add(iparam);

                iparam = new Parametros("@inEmitidioPor", PAGO["EmitidoPor"], Parametros.SType.VarChar);
                iParams.Add(iparam);

                iparam = new Parametros("@inLugarEmision", PAGO["LugarEmision"], Parametros.SType.VarChar);
                iParams.Add(iparam);

                //DateTime.ParseExact(miUtilerias.getPropertyValue("send-money-store-request.sender.compliance_details.date_of_birth", csObject), "ddMMyyyy", CultureInfo.InvariantCulture),
                iparam = new Parametros("@inFechaEmision", PAGO["FechaEmision"], Parametros.SType.DateTime);
                iParams.Add(iparam);

                iparam = new Parametros("@inFechaExpiracion", PAGO["FechaExpiracion"], Parametros.SType.DateTime);
                iParams.Add(iparam);

                ///***********************************************************************************************************/

                iparam = new Parametros("@inNacionalidad", PAGO["Nacionalidad"], Parametros.SType.VarChar);
                iParams.Add(iparam);

                iparam = new Parametros("@inDireccion1", PAGO["DirLinea1"], Parametros.SType.VarChar);
                iParams.Add(iparam);

                iparam = new Parametros("@inDireccion2", PAGO["DirLinea2"], Parametros.SType.VarChar);
                iParams.Add(iparam);

                iparam = new Parametros("@inIndicaciones", PAGO["Indicaciones"], Parametros.SType.VarChar);
                iParams.Add(iparam);

                iparam = new Parametros("@inSexo", PAGO["Sexo"], Parametros.SType.Char);
                iParams.Add(iparam);

                iparam = new Parametros("@inEstadoCivil", PAGO["EstadoCivil"], Parametros.SType.Char);
                iParams.Add(iparam);

                iparam = new Parametros("@inFechaNacimiento", PAGO["FechaNacimiento"], Parametros.SType.DateTime);
                iParams.Add(iparam);

                iparam = new Parametros("@inProfesion", PAGO["Profesion"], Parametros.SType.VarChar);
                iParams.Add(iparam);

                iparam = new Parametros("@inLugarTrabajo", PAGO["LugarTrabajo"], Parametros.SType.VarChar);
                iParams.Add(iparam);

                iparam = new Parametros("@inTipoTelefono", PAGO["TipoTelefono"], Parametros.SType.VarChar);
                iParams.Add(iparam);

                ///***********************************************************************************************************/

                iparam = new Parametros("@inNumeroTelefono", PAGO["NumeroTelefono"], Parametros.SType.VarChar);
                iParams.Add(iparam);

                iparam = new Parametros("@inDato1", PAGO["Dato1"], Parametros.SType.VarChar);
                iParams.Add(iparam);

                iparam = new Parametros("@inDato2", PAGO["Dato2"], Parametros.SType.VarChar);
                iParams.Add(iparam);

                iparam = new Parametros("@inClienteId", PAGO["ClienteID"], Parametros.SType.Int);
                iParams.Add(iparam);

                iparam = new Parametros("@inCtaAgencia", PAGO["CtaAgencia"], Parametros.SType.VarChar);
                iParams.Add(iparam);

                iparam = new Parametros("@inCtaBanco", PAGO["CtaBanco"], Parametros.SType.Int);
                iParams.Add(iparam);

                iparam = new Parametros("@inMonto", Convert.ToDecimal(PAGO["Monto"]), Parametros.SType.Decimal);
                iParams.Add(iparam);

                iparam = new Parametros("@inMontoML", PAGO["MontoML"], Parametros.SType.Decimal);
                iParams.Add(iparam);

                iparam = new Parametros("@inTasa", PAGO["Tasa"], Parametros.SType.Decimal);
                iParams.Add(iparam);

                iparam = new Parametros("@inCargo", Convert.ToDecimal(PAGO["Cargos"]), Parametros.SType.Decimal);
                iParams.Add(iparam);

                ///***********************************************************************************************************/

                iparam = new Parametros("@inCargosML", PAGO["CargosML"], Parametros.SType.Decimal);
                iParams.Add(iparam);

                iparam = new Parametros("@inCargosxEntrega", PAGO["CargosxEntrega"], Parametros.SType.Decimal);
                iParams.Add(iparam);

                iparam = new Parametros("@inCargosxEntregaML", PAGO["CargosxEntregaML"], Parametros.SType.Decimal);
                iParams.Add(iparam);

                iparam = new Parametros("@inCargosxMensaje", Convert.ToDecimal(PAGO["CargosxMensaje"]), Parametros.SType.Decimal);
                iParams.Add(iparam);

                iparam = new Parametros("@inCargosxMensajeML", PAGO["CargosxMensajeML"], Parametros.SType.Decimal);
                iParams.Add(iparam);

                iparam = new Parametros("@inImpuesto", PAGO["Impuesto"], Parametros.SType.Decimal);
                iParams.Add(iparam);

                iparam = new Parametros("@inImpuestoML", PAGO["ImpuestoML"], Parametros.SType.Decimal);
                iParams.Add(iparam);

                iparam = new Parametros("@inWUCard", PAGO["WUCard"], Parametros.SType.VarChar);
                iParams.Add(iparam);

                iparam = new Parametros("@inNumFact", int.Parse(PAGO["NumFact"]), Parametros.SType.Int);
                iParams.Add(iparam);

                iparam = new Parametros("@inNameFact", PAGO["NameFact"], Parametros.SType.VarChar);
                iParams.Add(iparam);

                ///***********************************************************************************************************/

                iparam = new Parametros("@inDirFact", PAGO["DirFact"], Parametros.SType.VarChar);
                iParams.Add(iparam);

                iparam = new Parametros("@inNIT", PAGO["NIT"], Parametros.SType.VarChar);
                iParams.Add(iparam);

                iparam = new Parametros("@inSerie", PAGO["Serie"], Parametros.SType.VarChar);
                iParams.Add(iparam);

                iparam = new Parametros("@inTipoEnvio", PAGO["TipoEnvio"], Parametros.SType.TinyInt);
                iParams.Add(iparam);

                iparam = new Parametros("@inEstatusAct", PAGO["EstatusAct"], Parametros.SType.Char);
                iParams.Add(iparam);

                iparam = new Parametros("@inEstatusSet", PAGO["EstatusSet"], Parametros.SType.Char);
                iParams.Add(iparam);

                iparam = new Parametros("@inUsuariId", PAGO["UsuarioID"], Parametros.SType.Int);
                iParams.Add(iparam);

                iparam = new Parametros("@inFechaEnvio", PAGO["FechaEnvio"], Parametros.SType.DateTime);
                iParams.Add(iparam);

                iparam = new Parametros("@inFecha", PAGO["Fecha"], Parametros.SType.DateTime);
                iParams.Add(iparam);

                iparam = new Parametros("@inFechaCreacion", PAGO["FechaCreacion"], Parametros.SType.DateTime);
                iParams.Add(iparam);

                //iparam = new Parametros("@inFechaCreacion", DateTime.ParseExact(miUtilerias.getPropertyValue("receive-money-pay-request.receiver.compliance_details.id_issue_date", csObject), "ddMMyyyy", CultureInfo.InvariantCulture), Parametros.SType.DateTime);
                //iParams.Add(iparam);
                /***********************************************************************************************************/

                /***********************************************************************************************************/

                var oCS = new UtileriasAWM().getAppSettingsKey("dbalias", _config);
                objDb = new MSSQLConnection(oCS);

                if (objDb.executeSP("remesadores.sp_GWMM_GuardarTransacciones", iParams, MSSQLConnection.ReturnTypes.Dataset))
                {
                    utiProxy.SaveLog(Convert.ToString(objDb), "1", key, 1, _config);
                    opResult = true;
                }
                else //if(!objDb.executeSP("remesadores.sp_GWMM_GuardarTransacciones", iParams, MSSQLConnection.ReturnTypes.Dataset))
                {
                    _errorMessage = objDb.getMessage();
                }
            }
            catch (Exception ex)
            {
                _errorMessage = ex.Message;
                opResult = true;
            }
            finally
            {
                if (objDb != null) objDb.closeConnection();
            }
            return opResult;
        }
    }
}
