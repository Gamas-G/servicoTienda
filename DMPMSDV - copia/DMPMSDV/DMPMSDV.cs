namespace DMPMSDV
{
    using System;
    using System.Diagnostics;
    using System.ServiceModel;
    using System.ServiceModel.Description;
    using System.ServiceProcess;
    using System.IO;
    using System.Configuration;
    using System.Text;
    using Encriptar;

    //ESTE ES NUESTRA CLASE QUE HEREDA DE ServiceBase
    public partial class DMPMSDV : ServiceBase
    {
        private ServiceHost m_svcHost = null; //VARIABLE DONDE CARGARA LA INSTANCIA DEL SERVICIO WCF CUANDO SE LEVANTE EL SERVICIO DE WINDOWS
        private static log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        //Variable para la conexión en central
        //DMPMSDVC.central.ConexSucClient conexionCentral;

        public DMPMSDV()
        {
            InitializeComponent();
        }

        /*public void OnDebug()
        {
            OnStart();
        }*/

        protected override void OnStart( string[] args )
        {
            log.Info(string.Format("El Microservicio de Directorios Virtuales ha iniciado"));
            //log.Info(string.Format( ConfigurationManager.AppSettings["catalogoDVs"] ));

            //Validando existencia de CatalogoDvS
            Monitoreo Mtr = new Monitoreo();
            
            Mtr.SincronizaCatalogoDVs();


            /*INSTANCIA DONDE VA LA LOGICA DEL MONITOREO*/
            /*
             * Se carga la configuración que tendra el servicio
             */
            Mtr.CargaConfiguracion();


            initialWCF();

            /*string msj = "[{\"parametrosId\":8,\"sucursal\":9797,\"workstation\":\"NT9797\",\"creacion\":\"2022-12-28\",\"detalle\":\"Falloenreconstruireldirectoriovirtualcredito\",\"estatusId\":2},{\"parametrosId\":8,\"sucursal\":2024,\"workstation\":\"NT2024\",\"creacion\":\"2022-12-28\",\"detalle\":\"FalloenreconstruireldirectoriovirtualDESTEC\",\"estatusId\":2},{\"parametrosId\":8,\"sucursal\":2244,\"workstation\":\"NT2244\",\"creacion\":\"2022-12-28\",\"detalle\":\"FalloenreconstruireldirectoriovirtualGerentePV\",\"estatusId\":2}]";

            log.Info("INSERTANDO HALLAZGO");
            DMPMSDVC.centraloGer.ConexSucClient conexSucClient = new DMPMSDVC.centraloGer.ConexSucClient();
            conexSucClient.ReportarHallazgos(msj);*/

            /*try
            {
            log.Info("DESENCRIPTANDO");
            string des = ReadCatalogoDVs(pathCatalogo);
            string mio = seguridad.Desencriptar(des);
            log.Info("OBTENEMOS \n" + mio);
            }
            catch (Exception msj)
            {
                log.Info("Error al desencriptar\n" + msj.Message);
            }


            try
            {

            
            WcfDMPMonitorDV wcfDMPMonitorDV = new WcfDMPMonitorDV();
            string msj = wcfDMPMonitorDV.RestauracionDV("DIRECTORIO GAMAS");
                log.Info("SE CONSUMIO: " + msj);
            }
            catch (Exception ex)
            {
                log.Info("Error en la restauración\n" + ex.Message);
            }*/


            /*log.Info("El catalogo tiene lo siguiente:\n" + Globales.catalogoDVDecrypt);

            log.Info("CONFIGURACIÓN \n" + Globales.HoraIniMtr + "INTERVALO :" +Globales.IntervaloMtr);*/


            /*DMPMSDVG.centraloGer.ConexSucClient centraloGer = new DMPMSDVG.centraloGer.ConexSucClient();
            string msj = centraloGer.HolaMundo() ;
            log.Info( "Obtuvimos del servicio de german" + msj );*/

            //WcfDMPMonitorDV m = new WcfDMPMonitorDV();
            //m.RestauracionDV( "NEWPOOLTEST" );


            /*
             * PREPARAMOS LAS TAREAS A TRABAJAR
             */
            tareasMonitoreo(Mtr);

            log.Info(string.Format("El Microservicio de Directorios Virtuales termino su proceso de inicio correctamente."));
        }

        protected override void OnStop()
        {
            if (m_svcHost != null)
            {
                m_svcHost.Close();
                m_svcHost = null;
            }
            TareasProgramadas.eliminarJobs();

            log.Info(string.Format("El Microservicio de Directorios Virtuales se ha detenido"));

            log.Logger.Repository.Shutdown();

            
        }

        public void initialWCF()
        {
            try
            {
                log.Info(string.Format("Se iniciara el WCF SerDirVirtual"));
                if (m_svcHost != null) m_svcHost.Close();

                //const string strAdrHTTP = "http://10.51.244.23:9001/DirVirtualMs";
                string strAdrHTTP = "http://" + Globales.ipNT + ":10004/ServDirVirtual";

                Uri[] adrbase = { new Uri(strAdrHTTP) };
                /*
                 * NOTA: 
                 *      AQUÍ SE COLOCARA LAS NUEVAS FUNCIONES PARA EL NUEVO SERVICIO DE CENTRAL
                 */
                m_svcHost = new ServiceHost(typeof(WcfDMPMonitorDV), adrbase); //Le colocamos la clase que contienen las funciones para el ENdPoint

                ServiceMetadataBehavior mBehave = new ServiceMetadataBehavior();
                m_svcHost.Description.Behaviors.Add(mBehave);

                BasicHttpBinding httpb = new BasicHttpBinding();
                /*httpb.MaxReceivedMessageSize = 2147483647;
                httpb.ReaderQuotas.MaxStringContentLength = 2147483647;*/
                m_svcHost.AddServiceEndpoint(typeof(IWcfDMPMonitorDV), httpb, strAdrHTTP);
                m_svcHost.AddServiceEndpoint(typeof(IMetadataExchange), MetadataExchangeBindings.CreateMexHttpBinding(), "mex");
                m_svcHost.Open();
                log.Info(string.Format("El servicio WCF DirVirtualMs se inicio correctamente"));
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Error en CargaDirVirtualMs: {0}", ex.Message));
            }
        }

        public void tareasMonitoreo( Monitoreo Mtr )
        {
            log.Info(string.Format("Se realizara la programación de las tareas de monitoreo"));
            TareasProgramadas TareasP = new TareasProgramadas();



            if (Globales.TipoMtrIni == "2") //Hora Programada y al iniciar WS
            {
                //Tarea para inicio de monitoreo
                TareasP.Monitoreo(3, "Monitoreo", "triggerMtr", string.Empty);
                log.Info(string.Format("Se validara monitoreo inicial"));

                DateTime myDate = DateTime.ParseExact(Globales.HoraIniMtr, "HH:mm", System.Globalization.CultureInfo.InvariantCulture);
                DateTime myDate2 = DateTime.ParseExact(DateTime.Now.Hour.ToString("00") + ":" + DateTime.Now.Minute.ToString("00"), "HH:mm", System.Globalization.CultureInfo.InvariantCulture);
                var minutes = (myDate - myDate2).TotalMinutes;
                if (minutes >= Globales.IntervMinMtr)
                {
                    log.Info(string.Format("Se realizara monitoreo inicial"));
                    Mtr.MonitorDV();
                }
                else if (minutes < 0)
                {
                    minutes = minutes * -1;
                    if (minutes <= Globales.ToleranciaMtr)
                    {
                        log.Info(string.Format("Se realizara monitoreo inicial"));
                        //Mtr.MonitorDV();
                        TareasP.Monitoreo(2, "CicloMtrDV", "triggerMtrDV", string.Empty);
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
                log.Info("Tipo de inicio 1");
                TareasP.Monitoreo(3, "Monitoreo", "triggerMtr", string.Empty);

                DateTime myDate = DateTime.ParseExact(Globales.HoraIniMtr, "HH:mm", System.Globalization.CultureInfo.InvariantCulture);
                DateTime myDate2 = DateTime.ParseExact(DateTime.Now.Hour.ToString("00") + ":" + DateTime.Now.Minute.ToString("00"), "HH:mm", System.Globalization.CultureInfo.InvariantCulture);
                var minutes = (myDate - myDate2).TotalMinutes;//obtiene los minutos transcurridos de la hora de apertura de la tienda
                if (minutes < 0)//Valida si ya paso la hora de apertura
                {
                    minutes = minutes * -1;
                    log.Info("El valor de minutes es: " + minutes);
                    log.Info("La minima tolerancia es de: " + Globales.ToleranciaMtr);
                    //if (minutes <= Globales.ToleranciaMtr)
                    //{
                        log.Info(string.Format("Se realizara monitoreo inicial"));
                        //Mtr.MonitorDV();
                        TareasP.Monitoreo(2, "CicloMtrDV", "triggerMtrDV", string.Empty);
                    //}
                    /*else
                    {
                        Mtr.InicioAlterno();
                    }*/
                }

            }
            else if (Globales.TipoMtrIni == "3") //Al iniciar la WS
            {
                log.Info(string.Format("Se realizara monitoreo inicial"));
                Mtr.MonitorDV();
            }//EJECUCION DIRECTA DEL MONITOREO DE DVs
        }

        //public void SincronizaCatalogoDVs()
        //{            
        //    /*
        //     * SI NO EXISTE EL CATALOGO, PROCEDEMOS A OBTENERLO DE CENTRAL
        //     */
        //    log.Info( "Validando existencia de catalogoDvs" );
        //    try
        //    {
        //        if (!File.Exists(pathCatalogo))
        //        {
        //            /*CONSUMIMOS EL SERVICIO WCF DE CENTRAL*/
        //            log.Info("CatalogoDVs no existe, iniciando conexión con gdtSyncDvTienda");

        //            DMPMSDVC.centraloGer.ConexSucClient cn = new DMPMSDVC.centraloGer.ConexSucClient();
        //            //DMPMSDV.centraloGer.ConexSucClient cn = new DMPMSDVC.centraloGer.ConexSucClient();
        //            jsonDVs = cn.SincronizarDvTienda();//Consultamos información con central
                    
        //            Globales.catalogoDVDecrypt = jsonDVs;//Guardamos la información en Globales para su uso directo
                    
        //            jsonDVsEncript = seguridad.Encriptar( jsonDVs );//Encriptando la información de catalogoDVs
        //            File.WriteAllText(pathCatalogo, jsonDVsEncript);//Creando el archivo, con la incriptación
        //            log.Info("CatalogoDVs encriptado correctamente");
        //        }
        //        else
        //        {
        //            log.Info("El catalogo existe, DESENCRIPTANDO y almacenando información para su trabajo");
        //            string contentFile = ReadCatalogoDVs( pathCatalogo );
        //            Globales.catalogoDVDecrypt = seguridad.Desencriptar( contentFile );
        //            log.Info( "Desencriptación exitosa" );

        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        log.Info(
        //                 "Metodo validaCatalogoDVs\n" +
        //                 "Ocurrio un error al sincronizar catalogoDVs con central: \n" + 
        //                 ex.Message 
        //                );
        //    }
        //}

        //public string ReadCatalogoDVs( string path )
        //{
        //    string fileInfo = string.Empty;
        //    try
        //    {
        //        fileInfo = File.ReadAllText( path );
        //    }
        //    catch (Exception msj)
        //    {
        //        log.Info("Ocurrio un erro al leer catalogoDVs\n Exception: " + msj.Message);
        //    }
        //    return fileInfo;
        //}

    }
}