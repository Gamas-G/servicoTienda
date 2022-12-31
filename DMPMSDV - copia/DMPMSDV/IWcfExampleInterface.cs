namespace DMPMSDV
{
    using System.ServiceModel;

    [ServiceContract]
    public interface IWcfDMPMonitorDV
    {
        /*[OperationContract] 
        bool MonitoreoDVPorSolicitud();*/
       
        [OperationContract]
        string RestauracionDV( int DvId );
    }
}  