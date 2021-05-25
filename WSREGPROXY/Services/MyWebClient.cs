using System;
using System.Collections.Generic;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace WSREGPROXY.Services
{
    public class MyWebClient : WebClient
    {
        public X509Certificate cert;
        protected override WebRequest GetWebRequest(Uri address)
        {
            HttpWebRequest request = (HttpWebRequest)base.GetWebRequest(address);
            try
            {
                request.ClientCertificates.Add(cert);

            }
            catch (Exception)
            { }
            return request;
        }
    }
}

