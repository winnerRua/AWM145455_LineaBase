using GrupoCoen.Corporativo.Libraries.AH2HAS.Utilidades;
using GrupoCoen.Corporativo.ProjectCenter.Libraries.CriptoEngine;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using WSREGGWMM.Entities;
using WSREGGWMM.Helpers;

namespace WSREGGWMM.Services
{
    public interface IPartnerService
    {
        Task<dynamic> Authenticate(IConfiguration config, string username, string password, int agencyID, string agencyCode = "", string producto = "", string paisValidacion = "");
        IDictionary<string, string> Tokens { get; }
    }

    public class PartnerService : IPartnerService
    {
        private readonly IDictionary<string, string> tokens = new Dictionary<string, string>();
        public IDictionary<string, string> Tokens => tokens;
        public async Task<dynamic> Authenticate(IConfiguration config, string username, string password, int agencyID, string agencyCode = "", string producto = "", string paisValidacion = "")
        {

            AS_Generales oGeneral = new AS_Generales();
            Partners partnerInfo = new Partners();
            string strError = string.Empty;
            var user = await Task.Run(() => new ConexionBD().GetPartnerUsuario(config, agencyID, agencyCode, ref strError, username, producto, paisValidacion));

            if (!string.IsNullOrEmpty(strError))
                return null;
            if (producto.Equals(string.Empty) && paisValidacion.Equals(string.Empty))
            {

                partnerInfo = (from query in user.AsEnumerable()
                               select new Partners
                               {
                                   PartnerId = query.Field<int>("PartnerID"),
                                   Username = query.Field<string>("UsuarioPartner"),
                                   Password = query.Field<string>("Password")
                               }).FirstOrDefault();
                if (string.IsNullOrEmpty(partnerInfo.Username) || string.IsNullOrEmpty(partnerInfo.Password) || partnerInfo.PartnerId == 0)
                    return null;

            }
            else
            {
                partnerInfo = (from query in user.AsEnumerable()
                               select new Partners
                               {
                                   Username = query.Field<string>("op"),
                                   Password = query.Field<string>("psw")
                               }).FirstOrDefault();

            }


            //Obtiene la llave
            BFEngine oDecripKey = new BFEngine();
            string oKey = oDecripKey.Process(oGeneral.mPasskey(), BFEngine.AccionCifrado.Decrypt);

            //se encripta el psw
            BFEngine oEncripTrama = new BFEngine(oKey);
            partnerInfo.Password = oEncripTrama.Process(partnerInfo.Password, BFEngine.AccionCifrado.Decrypt);


            if (username.CompareTo(partnerInfo.Username) != 0 || password.CompareTo(partnerInfo.Password) != 0)
                return null;

            var token = Guid.NewGuid().ToString();
            tokens.Add(token, partnerInfo.Username);

            return token;
        }
    }
}
