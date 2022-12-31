namespace DMPMSLR
{
    using System;

    public class WcfMsLR : IWcfMsLR
    {
        private static log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public bool MonitoreoLRporSolicitud()
        {
            bool MtrLR = false;

            try
            {
                log.Info(string.Format("Inicia proceso para realizar monitoreo de Llaves de Registro por demanda"));
                Monitoreo Mtr = new Monitoreo();
                MtrLR = Mtr.MonitorLR();
                log.Info(string.Format("Termina proceso para realizar monitoreo de Llaves de Registro por demanda"));
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Error en MonitoreoLRporSolicitud: {0}", ex.Message));
                return false;
            }

            return MtrLR;
        }
    }
}