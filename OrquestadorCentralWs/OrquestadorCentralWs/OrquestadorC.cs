namespace OrquestadorCentralWs
{
    using System;
    using System.ServiceModel;
    using System.ServiceModel.Description;
    using System.ServiceProcess;
    using DAL;
    using ML;
    using System.Diagnostics;
    using OrquestadorCentralWs.ServiceReference1;

    public partial class OrquestadorC : ServiceBase
    {
        private ServiceHost m_svcHost = null;
        private static log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public OrquestadorC()
        {
            InitializeComponent();
        }

        /*public void OnDebug()
        {
            OnStart();
        }*/

        protected override void OnStart( string[] args )
        {
            log.Info(string.Format("El Orquestador iniciara su carga"));

            //ConexSuc c = new ConexSuc();
            //c.CrearCatalogoDVs();
            //return;

            DBConex dbc = new DAL.DBConex();
            dbc.CargaConfiguracion();

            CargaOrqCentralWcf();

            log.Info( "Consumiendo nuevo servicio " );
            WcfDMPMonitorDVClient n = new WcfDMPMonitorDVClient();
            string test = n.RestauracionDV( "PRUEBAS GM" );

            log.Info( "El resutado de DMPMSDV es: " + test );
                        
            //TareasProgramadas TareasP = new TareasProgramadas();
            //Tarea para mantenimiento a tabla Cambios
            //TareasP.Monitoreo(1, "MantoC", "triggerMantoC", Globales.MantoTablaCmb);
            
            log.Info(string.Format("El Orquestador ha iniciado correctamente"));
        }

        protected override void OnStop()
        {
            if (m_svcHost != null)
            {
                m_svcHost.Close();
                m_svcHost = null;
            }
            log.Info(string.Format("El Orquestador se ha detenido"));
        }

        public void CargaOrqCentralWcf()
        {
            try
            {
                log.Info(string.Format("Se iniciara el WCF OrqCentralWcf"));
                if (m_svcHost != null) m_svcHost.Close();

                log.Info(string.Format("http://" + Globales.IpWs + ":9003/OrqCentralWcf"));
                string strAdrHTTP = "http://" + Globales.IpWs + ":9003/OrqCentralWcf";
                //string strAdrHTTP = "http://localhost:9003/OrqCentralWcf";

                Uri[] adrbase = { new Uri(strAdrHTTP) };
                m_svcHost = new ServiceHost(typeof(ConexSuc), adrbase);

                ServiceMetadataBehavior mBehave = new ServiceMetadataBehavior();
                m_svcHost.Description.Behaviors.Add(mBehave);

                BasicHttpBinding httpb = new BasicHttpBinding();
                httpb.MaxReceivedMessageSize = 2147483647;
                httpb.ReaderQuotas.MaxStringContentLength = 2147483647;
                m_svcHost.AddServiceEndpoint(typeof(OrquestadorCentralWs.IConexSuc), httpb, strAdrHTTP);
                m_svcHost.AddServiceEndpoint(typeof(IMetadataExchange),
                MetadataExchangeBindings.CreateMexHttpBinding(), "mex");
                m_svcHost.Open();
                log.Info(string.Format("OrqCentralWcf se inicio correctamente"));
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Error en CargaOrqCentralWcf: {0}", ex.Message));
            }
        }
    }
}