using System.ServiceModel;

namespace IntegrationService.Eispp
{
    [ServiceContract(Namespace = "http://IntegrationService.Eispp")]
    public interface IEisppService
    {
        [OperationContract]
        bool SendMessage(string message);

        [OperationContract]
        string[] ReceiveMessages();
    }
}
