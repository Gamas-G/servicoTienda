namespace OrquestadorCentralWs
{
    using System.ServiceModel;

    [ServiceContract]
    public interface IConexSuc
    {
        [OperationContract]
        bool EntregaRepHallazgos(string ContXml, int Suc);

        [OperationContract]
        string ConfigOrqAct(int Suc);

        [OperationContract]
        string SincronizarInfoMtr(int Suc, string FechaCambios);

        [OperationContract]
        bool ReportaEstacTrab(string WsCadena, int Suc);

        [OperationContract]
        string SolReportarHallazgoDemanda(string ip);

        [OperationContract]
        string SolZip(string ip, string ruta);

        [OperationContract]
        string CrearCatalogoDVs();
    }
}