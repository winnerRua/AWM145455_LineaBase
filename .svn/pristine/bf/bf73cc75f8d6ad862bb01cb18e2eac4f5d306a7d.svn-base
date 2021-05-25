namespace WSREGGWMM.Entities
{
    public class ErrorGateway
    {
        public string faultcode { get; set; }
        public string faultstring { get; set; }
        public string faultactor { get; set; }
        public Detail detail { get; set; }

    }

    public class Detail
    {
        public Airpak_error airpak_error { get; set; }

    }
    public class Airpak_error
    {
        public dynamic error { get; set; }
        public string ExternalReferenceNumber { get; set; }
        public string PartnerID { get; set; }
    }
}
