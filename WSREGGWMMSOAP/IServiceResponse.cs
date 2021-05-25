using System.ServiceModel;
using System.ServiceModel.Web;

namespace WSREGGWMMSOAP
{
    // NOTA: puede usar el comando "Rename" del menú "Refactorizar" para cambiar el nombre de interfaz "IService1" en el código y en el archivo de configuración a la vez.
    [ServiceContract]
    public interface IServiceResponse
    {

        [OperationContract]
        [WebGet]
        string GetServiceResponse(string xmlRequest);

        [OperationContract]
        [WebGet]
        string Authenticate(string xmlRequest);


    }
}
