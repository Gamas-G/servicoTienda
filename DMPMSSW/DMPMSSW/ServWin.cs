namespace DMPMSSW
{
    using System;
    using System.Diagnostics;
    using System.ServiceModel;
    using System.ServiceModel.Description;
    using System.ServiceProcess;

    public partial class ServWin : ServiceBase
    {
        private ServiceHost m_svcHost = null;
        private static log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public ServWin()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            log.Info(string.Format("El Microservicio de Servicios Windows ha iniciado y se cargara la configuración"));
            Monitoreo Mtr = new Monitoreo();
            Mtr.CargaConfiguracion();
            
            CargaWcfMsSW();
            
            log.Info(string.Format("Se realizara la programación de las tareas de monitoreo"));
            TareasProgramadas TareasP = new TareasProgramadas();

            if (Globales.TipoMtrIni == "2") //Hora Programada y al iniciar WS
            {
                //Tarea para inicio de monitoreo
                TareasP.Monitoreo(3, "MonitoreoSW", "triggerMtrSW", string.Empty);
                log.Info(string.Format("Se validara monitoreo inicial"));
                
                DateTime myDate = DateTime.ParseExact(Globales.HoraIniMtr, "HH:mm", System.Globalization.CultureInfo.InvariantCulture);
                DateTime myDate2 = DateTime.ParseExact(DateTime.Now.Hour.ToString("00") + ":" + DateTime.Now.Minute.ToString("00"), "HH:mm", System.Globalization.CultureInfo.InvariantCulture);
                var minutes = (myDate - myDate2).TotalMinutes;
                if (minutes >= Globales.IntervMinMtr)
                {
                    log.Info(string.Format("Se realizara monitoreo inicial"));
                    Mtr.MonitorSW();
                }
                else if (minutes < 0)
                {
                    minutes = minutes * -1;
                    if (minutes <= Globales.ToleranciaMtr)
                    {
                        log.Info(string.Format("Se realizara monitoreo inicial"));
                        TareasP.Monitoreo(2, "CicloMtrSW", "triggerCMtrSW", string.Empty);
                    }
                    else
                    {
                        Mtr.InicioAlterno();
                    }
                }
            }
            else if (Globales.TipoMtrIni == "1") //Hora Programada
            {
                //Tarea para inicio de monitoreo
                TareasP.Monitoreo(3, "MonitoreoSW", "triggerMtrSW", string.Empty);
                #region Validar monitoreo inicial
                DateTime myDate = DateTime.ParseExact(Globales.HoraIniMtr, "HH:mm", System.Globalization.CultureInfo.InvariantCulture);
                DateTime myDate2 = DateTime.ParseExact(DateTime.Now.Hour.ToString("00") + ":" + DateTime.Now.Minute.ToString("00"), "HH:mm", System.Globalization.CultureInfo.InvariantCulture);
                var minutes = (myDate - myDate2).TotalMinutes;
                if (minutes < 0)
                {
                    minutes = minutes * -1;
                    if (minutes <= Globales.ToleranciaMtr)
                    {
                        log.Info(string.Format("Se realizara monitoreo inicial"));
                        TareasP.Monitoreo(2, "CicloMtrSW", "triggerCMtrSW", string.Empty);
                    }
                    else
                    {
                        Mtr.InicioAlterno();
                    }
                }
                #endregion
            }
            else if (Globales.TipoMtrIni == "3") //Al iniciar la WS
            {
                log.Info(string.Format("Se realizara monitoreo inicial"));
                Mtr.MonitorSW();
            }
            log.Info(string.Format("El Microservicio de Servicios Windows termino su proceso de inicio correctamente."));
        }

        protected override void OnStop()
        {
            if (m_svcHost != null)
            {
                m_svcHost.Close();
                m_svcHost = null;
            }
            log.Info(string.Format("El Microservicio de Servicios Windows se ha detenido"));
        }

        public void CargaWcfMsSW()
        {
            try
            {
                log.Info(string.Format("Se iniciara el WCF ServWinMs"));
                if (m_svcHost != null) m_svcHost.Close();

                string strAdrHTTP = "http://" + Globales.ipNT + ":10008/ServWinMs";

                Uri[] adrbase = { new Uri(strAdrHTTP) };
                m_svcHost = new ServiceHost(typeof(WcfMsSW), adrbase);

                ServiceMetadataBehavior mBehave = new ServiceMetadataBehavior();
                m_svcHost.Description.Behaviors.Add(mBehave);

                BasicHttpBinding httpb = new BasicHttpBinding();
                m_svcHost.AddServiceEndpoint(typeof(IWcfMsSW), httpb, strAdrHTTP);
                m_svcHost.AddServiceEndpoint(typeof(IMetadataExchange), MetadataExchangeBindings.CreateMexHttpBinding(), "mex");
                m_svcHost.Open();
                log.Info(string.Format("ServWinMs se inicio correctamente"));
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Error en CargaWcfMsSW: {0}", ex.Message));
            }
        }



    }
}