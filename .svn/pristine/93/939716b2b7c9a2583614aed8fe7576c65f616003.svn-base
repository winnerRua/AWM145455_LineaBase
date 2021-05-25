using Newtonsoft.Json;
using System;
using System.Net;
using System.Net.Http.Headers;
using System.Web;
using System.Xml;
using System.Xml.Linq;

namespace WSREGGWMMSOAP
{
    // NOTA: puede usar el comando "Rename" del menú "Refactorizar" para cambiar el nombre de clase "Service1" en el código, en svc y en el archivo de configuración.
    // NOTE: para iniciar el Cliente de prueba WCF para probar este servicio, seleccione Service1.svc o Service1.svc.cs en el Explorador de soluciones e inicie la depuración.
    public class ServiceResponse : IServiceResponse
    {
        //variables que se usan en el metodo de conversion del xml a json y viseversa
        string Converjson, Converxml;

        public string GetServiceResponse(string xmlRequest)
        {
            return "";
        }


        public string Authenticate(string xmlRequest)
        {
            //---------------------------------------------------------------------

            if (HttpContext.Current.Request.Headers["Authorization"] == null)
            { return "Mensaje error"; }
            else
            {
                //Metodo para convertir de XML a JSON
                ConvertXmlJson();

                //Metodo para convertir de JSON a XML
                ConvertJsonXml();

                //metodoserviceResponse();
            }

            var authHeader = AuthenticationHeaderValue.Parse(HttpContext.Current.Request.Headers["Authorization"]);

            var credentialBytes = Convert.FromBase64String(authHeader.Parameter);
            var credentials = System.Text.Encoding.UTF8.GetString(credentialBytes).Split(new[] { ':' }, 2);
            var username = credentials[0];
            var password = credentials[1];

            //using (HttpClient MockClient = new HttpClient())
            //{
            //    MockClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(username, password);
            //    var response = MockClient.GetAsync("https://localhost:44318/api/gateway/authenticate").Result;
            //    var resultadoServicio = response.Content.ReadAsStringAsync().Result;
            //}

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            //CONSUMIR SERVICIO AUTHENTICATE
            WebRequest hwrequest = WebRequest.Create("https://localhost:44318/api/gateway/authenticate");
            hwrequest.Headers.Add("AUTHORIZATION", "Basic");
            hwrequest.Credentials = new NetworkCredential(username, password);

            hwrequest.ContentType = "application/json";
            hwrequest.Method = "POST";

            System.Text.ASCIIEncoding encoding = new System.Text.ASCIIEncoding();
            //se pasa el json resultante del xml
            byte[] postByteArray = encoding.GetBytes("{\"agencyCode\" : \"AGC139435\"}");//Converjson);
            hwrequest.ContentLength = postByteArray.Length;

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            System.IO.Stream postStream = hwrequest.GetRequestStream();
            postStream.Write(postByteArray, 0, postByteArray.Length);
            postStream.Close();

            try
            {
                WebResponse hwresponse = hwrequest.GetResponse();
                System.IO.StreamReader responseStream = new System.IO.StreamReader(hwresponse.GetResponseStream());
                var strReply = responseStream.ReadToEnd();
                hwresponse.Close();
            }

            catch (WebException ex)
            {

            }

            //Consumir servicio serviceResponse
            metodoserviceResponse();

            return "";

        }



        public void ConvertXmlJson()
        {
            //---------------------------------------------------\\
            //Convertir de XML a JSON                            \\
            //---------------------------------------------------\\
            string xml = @"<root>
                      <PartnerId>1</PartnerId>
                      <Username>Alan</Username>
                      <Password>1234</Password>
                    </root>";

            var schemaFileName = HttpContext.Current.Request.Headers["Authorization"];

            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xml);
            Converjson = JsonConvert.SerializeXmlNode(doc);
            Console.WriteLine(Converjson + "el docuemto xml es valido");
        }

        public void ConvertJsonXml()
        {
            //---------------------------------------------------\\
            //Convertir de JSON a XML                            \\
            //---------------------------------------------------\\ 

            Converxml = "{    \"?xml\": {        \"@version\": \"1.0\",        \"@standalone\": \"no\"    },    \"soapenv:Envelope\": {        \"@xmlns:soapenv\": \"http://schemas.xmlsoap.org/soap/envelope/\",        \"PartnerId\": 1,        \"Username\": \"User\",        \"Password\": 1234    }    }";
            XDocument node = JsonConvert.DeserializeXNode(Converxml, "", true);
            //XmlDocument xmlDoc = JsonConvert.DeserializeXmlNode(Converxml, "", true);
            Console.WriteLine(node.ToString());
        }

        public void metodoserviceResponse()
        {


            //var authHeader = HttpContext.Current.Request.Headers["IdentityUrl"];
            //var identityUrl = HttpContext.Current.Request.Headers["IdentityUrl"];
            //var credentialBytes = Convert.FromBase64String(authHeader);
            //var credentials = System.Text.Encoding.UTF8.GetString(credentialBytes).Split(new[] { ':' }, 1);
            //var username = credentials[0];
            //ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            //WebRequest hwrequest = WebRequest.Create("https://localhost:44318/api/gateway/serviceResponse");
            //hwrequest.Headers.Add("IdentityUrl","");
            //var authenticationProviderKey = "IdentityApiKey";

            //hwrequest.ContentType = "application/json";
            ////string postData = "82217e71-b77b-4c84-8a39-89deb3aaffa4";
            //hwrequest.Method = "POST";
            //ASCIIEncoding encoding = new ASCIIEncoding();
            ////byte[] data = encoding.GetBytes(postData);
            //byte[] data = encoding.GetBytes("{\"AGC139435\"}");
            //hwrequest.ContentLength = data.Length;
            //Stream newStream = hwrequest.GetRequestStream();
            //newStream.Write(data, 0, data.Length);
            //newStream.Close();

            var authHeader = HttpContext.Current.Request.Headers["Authorization"];

            var credentialBytes = Convert.FromBase64String(authHeader);
            var credentials = System.Text.Encoding.UTF8.GetString(credentialBytes).Split(new[] { ':' }, 2);
            var username = credentials[0];
            //aqui el error
            var password = credentials[1];

            WebRequest hwrequest = WebRequest.Create("https://localhost:44318/api/gateway/serviceResponse");
            hwrequest.ContentType = "application/json";
            string postData = "82217e71-b77b-4c84-8a39-89deb3aaffa4";
            hwrequest.Method = "POST";

            System.Text.ASCIIEncoding encoding = new System.Text.ASCIIEncoding();
            //se pasa el json resultante del xml
            byte[] postByteArray = encoding.GetBytes(postData);
            hwrequest.ContentLength = postByteArray.Length;

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            System.IO.Stream postStream = hwrequest.GetRequestStream();
            postStream.Write(postByteArray, 0, postByteArray.Length);
            postStream.Close();

            try
            {
                WebResponse hwresponse = hwrequest.GetResponse();
                System.IO.StreamReader responseStream = new System.IO.StreamReader(hwresponse.GetResponseStream());
                var strReply = responseStream.ReadToEnd();
                hwresponse.Close();
            }

            catch (WebException ex)
            {

            }



            //using (HttpClient MockClient = new HttpClient())
            //{
            //    var response = MockClient.GetAsync("https://localhost:44318/api/gateway/serviceResponse").Result;
            //    var resultadoServicio = response.Content.ReadAsStringAsync().Result;
            //}
        }


    }
}
