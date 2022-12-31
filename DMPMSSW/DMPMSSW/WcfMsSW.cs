namespace DMPMSSW
{
    using System;
    using System.Diagnostics;

    public class WcfMsSW : IWcfMsSW
    {
        private static log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public bool MonitoreoSWporSolicitud()
        {
            bool MtrSW = false;

            try
            {
                log.Info(string.Format("Inicia proceso para realizar monitoreo de Servicios Windows por demanda"));
                Monitoreo Mtr = new Monitoreo();
                MtrSW = Mtr.MonitorSW();
                log.Info(string.Format("Termina proceso para realizar monitoreo de Servicios Windows por demanda"));
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Error en MonitoreoSWporSolicitud: {0}", ex.Message));
                return false;
            }

            return MtrSW;
        }
        
    }
}