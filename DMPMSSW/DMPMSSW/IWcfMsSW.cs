namespace DMPMSSW
{
    using System.ServiceModel;

    [ServiceContract]
    public interface IWcfMsSW
    {
        [OperationContract]
        bool MonitoreoSWporSolicitud();
    }
}