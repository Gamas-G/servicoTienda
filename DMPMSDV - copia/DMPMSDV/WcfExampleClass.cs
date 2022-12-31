namespace DMPMSDV
{
    using System;
    using System.Diagnostics;
    using Newtonsoft.Json;

    class WcfDMPMonitorDV : IWcfDMPMonitorDV
    {
        private static log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /*public bool MonitoreoDVPorSolicitud()
        {
            bool MtrDV = false;
            try
            {
                log.Info(string.Format("Inicia proceso para realizar monitoreo de Directorios Virtuales por demanda"));
                Monitoreo Mtr = new Monitoreo();
                MtrDV = Mtr.MonitorDV();
                log.Info(string.Format("Termina proceso para realizar monitoreo de Directorios Virtuales por demanda"));
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Error en MonitoreoDVPorSolicitud: {0}", ex.Message));
                return false;
            }

            return MtrDV;
        }*/

        public string RestauracionDV( int DvId )
        {
            string jsonSerializado;
            try
            {
                //log.Info("Restaurando Directorio Virtual " + nombreDV);
                model.Hallazgos hallazgos = new model.Hallazgos()
                {
                    Hallazgo = new model.Hallazgo()
                    {
                        DvId = 8,
                        Sucursal = 9797,
                        Workstation = "NT0087",
                        FechaReporte = "2022-12-28",
                        Detalle = $"Recosntrucción del directorio virtual {DvId}, por peticion del usuario, EXITOSO",
                        EstatusId = 2
                    }
                    /*Hallazgo = new model.Hallazgo[] {
                        new model.Hallazgo(){
                            ParametrosId = 8,
                            Sucursal     = 9797,
                            Workstation  = "NT0087",
                            Creacion     = "2022-12-28",
                            Detalle      = "Recosntrucción del directorio virtual exitoso",
                            EstatusId    = 2
                        },
                        new model.Hallazgo(){
                            ParametrosId = 8,
                            Sucursal     = 9797,
                            Workstation  = "NT0087",
                            Creacion     = "2022-12-28",
                            Detalle      = "No se encontro el Directorio Virtual SisCam del sitio Default Web Site",
                            EstatusId    = 2
                        }
                    }*/
                };
                jsonSerializado = JsonConvert.SerializeObject( hallazgos );
                log.Info( "Retornamos el siguiente mensaje " + jsonSerializado );
                return jsonSerializado;
            }
            catch (Exception ex)
            {
                log.Info( "Error en RestauracionDV " + ex.Message);
                return "Error en RestauracionDV \n" + ex.Message;
            }
        }

    }
}