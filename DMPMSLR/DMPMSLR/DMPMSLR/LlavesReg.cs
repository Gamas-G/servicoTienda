namespace DMPMSLR
{
    using System;
    using System.ServiceModel;
    using System.ServiceModel.Description;
    using System.ServiceProcess;

    public partial class LlavesReg : ServiceBase
    {
        private ServiceHost m_svcHost = null;
        private static log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public LlavesReg()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            #region Carga de Configuración            
            log.Info(string.Format("El Microservicio de Llaves de Registro ha iniciado y se cargara la configuración"));
            Monitoreo Mtr = new Monitoreo();
            Mtr.CargaConfiguracion();
            #endregion
            #region Carga WCF
            CargaWcfMsLR();
            #endregion
            #region Programación de Tareas de Monitoreo
            log.Info(string.Format("Se realizara la programación de las tareas de monitoreo"));
            TareasProgramadas TareasP = new TareasProgramadas();

            if (Globales.TipoMtrIni == "2") //Hora Programada y al iniciar WS
            {
                //Tarea para inicio de monitoreo
                TareasP.Monitoreo(3, "MonitoreoLR", "triggerMtrLR", string.Empty);
                log.Info(string.Format("Se validara monitoreo inicial"));
                #region Validar monitoreo inicial
                DateTime myDate = DateTime.ParseExact(Globales.HoraIniMtr, "HH:mm", System.Globalization.CultureInfo.InvariantCulture);
                DateTime myDate2 = DateTime.ParseExact(DateTime.Now.Hour.ToString("00") + ":" + DateTime.Now.Minute.ToString("00"), "HH:mm", System.Globalization.CultureInfo.InvariantCulture);
                var minutes = (myDate - myDate2).TotalMinutes;
                if (minutes >= Globales.IntervMinMtr)
                {
                    log.Info(string.Format("Se realizara monitoreo inicial"));
                    Mtr.MonitorLR();
                }
                else if (minutes < 0)
                {
                    minutes = minutes * -1;
                    if (minutes <= Globales.ToleranciaMtr)
                    {
                        log.Info(string.Format("Se realizara monitoreo inicial"));
                        TareasP.Monitoreo(2, "CicloMtrLR", "triggerCMtrLR", string.Empty);
                    }
                    else
                    {
                        Mtr.InicioAlterno();
                    }
                }
                #endregion
            }
            else if (Globales.TipoMtrIni == "1") //Hora Programada
            {
                //Tarea para inicio de monitoreo
                TareasP.Monitoreo(3, "MonitoreoLR", "triggerMtrLR", string.Empty);
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
                        TareasP.Monitoreo(2, "CicloMtrLR", "triggerCMtrLR", string.Empty);
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
                Mtr.MonitorLR();
            }
            #endregion
            log.Info(string.Format("El Microservicio de Llaves de Registro termino su proceso de inicio correctamente."));
        }

        protected override void OnStop()
        {
            if (m_svcHost != null)
            {
                m_svcHost.Close();
                m_svcHost = null;
            }
            log.Info(string.Format("El Microservicio de Llaves de Registro se ha detenido"));
        }

        public void CargaWcfMsLR()
        {
            try
            {
                log.Info(string.Format("Se iniciara el WCF LlavesRegMs"));
                if (m_svcHost != null) m_svcHost.Close();

                string strAdrHTTP = "http://" + Globales.ipNT + ":10006/LlavesRegMs";

                Uri[] adrbase = { new Uri(strAdrHTTP) };
                m_svcHost = new ServiceHost(typeof(WcfMsLR), adrbase);

                ServiceMetadataBehavior mBehave = new ServiceMetadataBehavior();
                m_svcHost.Description.Behaviors.Add(mBehave);

                BasicHttpBinding httpb = new BasicHttpBinding();
                m_svcHost.AddServiceEndpoint(typeof(IWcfMsLR), httpb, strAdrHTTP);
                m_svcHost.AddServiceEndpoint(typeof(IMetadataExchange), MetadataExchangeBindings.CreateMexHttpBinding(), "mex");
                m_svcHost.Open();
                log.Info(string.Format("LlavesRegMs se inicio correctamente"));
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Error en CargaWcfMsLR: {0}", ex.Message));
            }
        }
    }
}