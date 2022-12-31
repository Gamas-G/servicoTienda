namespace DMPMSLR
{
    using System.ServiceModel;

    [ServiceContract]
    public interface IWcfMsLR
    {
        [OperationContract]
        bool MonitoreoLRporSolicitud();
    }
}