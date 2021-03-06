using System;
using System.Collections.Generic;
using System.Text;

namespace WSREGAWM.Entities
{
    class WorkFlows
    {
        public string NombreWorkFlow { get; set; }
        public int WorkFlowID { get; set; }
        public bool ConsumeAutorizador { get; set; }
        public bool ConsumeProxy { get; set; }

        public bool GuardaTransaccion { get; set; }
        //public int RemesadorID { get; set; }
        public bool ValidaSGR { get; set; }

        public bool AlmacenaSGR { get; set; }

        public int ProductoID { get; set; }
        public string CounterID { get; set; }
        public string TerminalID { get; set; }
        public string AgenciaCuenta { get; set; }
        public int IDAgencia { get; set; }
        public string Usuario { get; set; }
        public string Contraseña { get; set; }
        public List<Operacion> Operaciones { get; set; }

        public WorkFlows()
        {
            NombreWorkFlow = "";
            WorkFlowID = 0;
            ConsumeAutorizador = false;
            ConsumeProxy = false;
            GuardaTransaccion = false;
            ValidaSGR = false;
            AlmacenaSGR = false;

            ProductoID = 0;
            CounterID = "";
            TerminalID = "";
            AgenciaCuenta = "";
            IDAgencia = 0;
            Usuario = "";
            Contraseña = "";
            //RemesadorID = 0;
            Operaciones = new List<Operacion>();
        }
    }

    class Operacion
    {
        public string NombreOperacion { get; set; }
        public bool actConsumeAutorizador { get; set; }
        public bool actConsumeProxy { get; set; }

        public Operacion()
        {
            NombreOperacion = string.Empty;
            actConsumeAutorizador = false;
            actConsumeProxy = false;
        }
    }

}
