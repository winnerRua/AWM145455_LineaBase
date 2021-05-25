using System;
using System.Collections.Generic;
using System.Text;

namespace WSREGPROXY.Entities
{
    public class LogGateway
    {
        public int ComponenteID { get; set; }
        public string KeyEvento { get; set; }
        public string Metadata { get; set; }
        public string IdentificadorEvento { get; set; }
    }
}
