using System;
using System.Net.Http.Headers;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Web;

namespace WSREGGWMMSOAP
{
    public class BasicAuthHttpModule : IHttpModule
    {
        private const string Realm = "My Realm";


        public void Init(HttpApplication context)
        {
            // Register event handlers
            context.AuthenticateRequest += OnApplicationAuthenticateRequest;
            context.EndRequest += OnApplicationEndRequest;
        }

        //--------------------------------------------------------------------------
        private static void SetPrincipal(IPrincipal principal)
        {
            Thread.CurrentPrincipal = principal;
            if (HttpContext.Current != null)
            {
                HttpContext.Current.User = principal;
            }
        }

        // TODO: Here is where you would validate the username and password.
        private static bool CheckPassword(string username, string password)
        {
            //var authHeader = AuthenticationHeaderValue.Parse(HttpContext.Current.Request.Headers["Authorization"]);
            //var credentialBytes = Convert.FromBase64String(authHeader.Parameter);
            //var credentials = System.Text.Encoding.UTF8.GetString(credentialBytes).Split(new[] { ':' }, 2);
            //var Username = credentials[0];
            //var Password = credentials[1];
            //return username == Username && password == Password;

            return username == "h2htest" && password == "Temp123*";


        }

        private static void AuthenticateUser(string credentials)
        {
            try
            {
                var encoding = Encoding.GetEncoding("iso-8859-1");
                credentials = encoding.GetString(Convert.FromBase64String(credentials));

                int separator = credentials.IndexOf(':');
                string name = credentials.Substring(0, separator);
                string password = credentials.Substring(separator + 1);

                if (CheckPassword(name, password))
                {
                    var identity = new GenericIdentity(name);
                    SetPrincipal(new GenericPrincipal(identity, null));
                }
                else
                {
                    // Invalid username or password.
                    HttpContext.Current.Response.StatusCode = 401;
                }
            }
            catch (FormatException)
            {
                // Credentials were not formatted correctly.
                HttpContext.Current.Response.StatusCode = 401;
            }
        }
        //--------------------------------------------------------------------------


        private static void OnApplicationAuthenticateRequest(object sender, EventArgs e)
        {
            var request = HttpContext.Current.Request;
            var authHeader = request.Headers["Authorization"];
            if (authHeader != null)
            {
                var authHeaderVal = AuthenticationHeaderValue.Parse(authHeader);

                // RFC 2617 sec 1.2, "scheme" name is case-insensitive
                if (authHeaderVal.Scheme.Equals("basic",
                        StringComparison.OrdinalIgnoreCase) &&
                    authHeaderVal.Parameter != null)
                {
                    AuthenticateUser(authHeaderVal.Parameter);
                }
            }

            //HttpApplication app = (HttpApplication)sender;
            //var operacion = app.Context.Request.HttpMethod.ToString();
            //var authHeader = app.Request.Headers["Authorization"];

            //if (HttpContext.Current.Request.Headers["SOAPAction"] != null && HttpContext.Current.Request.Headers["SOAPAction"].Contains("Authenticate"))
            //        operacion = "GET";

            //switch (operacion) {
            //    case "GET":
            //        OnEndRequest(sender, e);
            //        return;
            //    case "POST":
            //        if (authHeader == null)
            //        {
            //            DenyAccess(app);
            //            return;
            //        }

            //        if (!authHeader.StartsWith("bearer", StringComparison.OrdinalIgnoreCase))
            //        {

            //            DenyAccess(app);
            //            return;
            //        }

            //        var token = authHeader.Substring("bearer".Length).Trim();

            //        if (string.IsNullOrEmpty(token))
            //        {
            //            DenyAccess(app);
            //            return;
            //        }
            //        return;
            //    default:
            //        DenyAccess(app);
            //        return;

            //}
        }

        // If the request was unauthorized, add the WWW-Authenticate header 
        private static void OnApplicationEndRequest(object sender, EventArgs e)
        {
            var response = HttpContext.Current.Response;
            if (response.StatusCode == 401)
            {
                response.Headers.Add("WWW-Authenticate",
                    string.Format("Basic realm=\"{0}\"", Realm));
            }
        }
        private static void DenyAccess(HttpApplication app)
        {
            app.Response.StatusCode = 401;
            app.Response.StatusDescription = "Access Denied";
            app.Response.Write("401 Access Denied");
            app.CompleteRequest();
        }
        private static void OnEndRequest(object source, EventArgs eventArgs)
        {
            if (HttpContext.Current.Response.StatusCode == 401)
            {
                HttpContext context = HttpContext.Current;
                context.Response.StatusCode = 401;
                context.Response.AddHeader("WWW-Authenticate", "Basic Realm");
            }
        }

        public void Dispose()
        {
        }
    }
}