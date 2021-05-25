using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using WSREGPROXY.Entities;
using WSREGPROXY.Helpers;

namespace WSREGPROXY.Services
{
    public class SaveLogDataDelegate
    {
        private static string errorMessage = "";

        private delegate void Log_SaveLogDataDelegate(dynamic pMetadata, string identificadorEvento, string keyEvent, int ComponentID, IConfiguration _config);
        public static void SaveLogDataDelegateAsync(dynamic pMetadata, string identificadorEvento, string keyEvent, int ComponentID, IConfiguration _config)
        {
            Log_SaveLogDataDelegate delegado = new Log_SaveLogDataDelegate(SaveLogDelegate);
            delegado.BeginInvoke(pMetadata, identificadorEvento, keyEvent, ComponentID, _config, null, null);
        }
        public static void SaveLogDelegate(dynamic pMetadata, string identificadorEvento, string keyEvent, int ComponentID, IConfiguration _config)
        {
            try
            {
                string json = string.Empty;
                string uri = string.Empty;
                uri = new UtileriasProxy().getAppSettingsKey("servicioLogs", _config, "Servicios");
                var JWT = new UtileriasProxy().GenerarJWTLog(_config);
                var httpWebRequest = (HttpWebRequest)WebRequest.Create(uri);
                httpWebRequest.ContentType = "application/json";
                httpWebRequest.Method = "POST";
                httpWebRequest.Headers.Add("Authorization", "Bearer " + JWT);

                LogGateway objlogateway = new LogGateway()
                {
                    Metadata = Convert.ToString(pMetadata),
                    KeyEvento = keyEvent,
                    ComponenteID = ComponentID,
                    IdentificadorEvento = identificadorEvento
                };
                using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                {
                    json = JsonConvert.SerializeObject(objlogateway);
                    streamWriter.Write(json);
                }

                var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var result = streamReader.ReadToEnd();
                };
            }
            catch
            {
            }
        }
    }

}
