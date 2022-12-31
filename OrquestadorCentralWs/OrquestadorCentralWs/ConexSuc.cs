namespace OrquestadorCentralWs
{
    using System;
    using DAL;
    using System.ServiceModel;
    using ConexionTiendas;
    using System.Diagnostics;
    using Newtonsoft.Json;

    class ConexSuc : IConexSuc
    {
        private static log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public bool EntregaRepHallazgos(string ContXml, int Suc)
        {
            bool SeGuardo = false;

            try
            {
                log.Info("EntregaRepHallazgos - Inicia proceso de guardado de Hallazgos Sucursal: " + Suc.ToString());
                DBConex dbc = new DAL.DBConex();
                dbc.GuardaMasivaHallaz(ContXml, Suc);//Inserta los hallazgos enviados de Sucursal
                dbc.CheckRepHallaz(1, Suc);//Actualiza el cambo fdFechaRespHallz de la tabla DMPMtrControl de la sucursal

                log.Info("EntregaRepHallazgos - Termina proceso de guardado de Hallazgos Sucursal: " + Suc.ToString());
                SeGuardo = true;
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Error en EntregaRepHallazgos: {0}", ex.Message));
                return SeGuardo;
            }

            return SeGuardo;
        }

        public string ConfigOrqAct(int Suc)
        {
            string Respcfg = string.Empty;
            try
            {
                log.Info("ConfigOrqAct - Inicia busqueda de datos de configuración de la Sucursal: " + Suc.ToString());
                DBConex dbc = new DAL.DBConex();
                Respcfg = dbc.BusqCfgOrqSuc(1, Suc);
                log.Info("ConfigOrqAct - Se enviaron datos de configuración a la Sucursal: " + Suc.ToString());
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Error en EntregaRepHallazgos: {0}", ex.Message));
                return ex.Message;
            }
            return Respcfg;
        }

        public string SincronizarInfoMtr(int Suc, string FechaCambios)
        {
            string RespXml = string.Empty;
            try
            {
                log.Info("SincronizarMtrInfo - Inicia sincronización de información de Monitoreo con Sucursal: " + Suc.ToString());
                DBConex dbc = new DAL.DBConex();
                RespXml = dbc.ObtCambios(FechaCambios,Suc);
                dbc.CheckRepHallaz(2, Suc);
                log.Info("SincronizarMtrInfo - Se enviaron cambios en información de Monitoreo a la Sucursal: " + Suc.ToString());
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Error en SincronizarMtrInfo: {0}", ex.Message));
                return ex.Message;
            }
            return RespXml;
        }

        public bool ReportaEstacTrab(string WsCadena, int Suc)
        {
            bool SeGuardo = false;
            try
            {
                log.Info("ReportaEstacTrab - Inicia actualización de Estaciones en EstacTrab de Sucursal: " + Suc.ToString());
                DBConex dbc = new DAL.DBConex();
                SeGuardo = dbc.GuardaEstacTrab(WsCadena, Suc);
                log.Info("ReportaEstacTrab - Termina actualización de Estaciones en EstacTrab de Sucursal: " + Suc.ToString());
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Error en ReportaEstacTrab: {0}", ex.Message));
                return SeGuardo;
            }
            return SeGuardo;
        }


        public string SolReportarHallazgoDemanda(string ip)
        {
            try
            {
                log.Info("Ingresa SolReportarHallazgoDemanda");
                ConexCentralClient peticion = CreaReferenciaServicioDV(ip);
                log.Info("Petición...");
                peticion.SolReportarHallazgos();
                log.Info("Cerrando petición...");
                peticion.Close();
                log.Info("Termina SolReportarHallazgoDemanda");
                
            }
            catch (Exception ex)
            {
                log.Error(ex.ToString());
            }
            return "Finalizado";
        }

        public string SolZip(string ip, string ruta)
        {
            var resp = "error";
            try
            {
                log.Info("Ingresa SolReportarHallazgoDemanda");
                ConexCentralClient peticion = CreaReferenciaServicioDV(ip);
                log.Info("Petición...");
                resp = peticion.AnyforToWork(ruta);

                log.Info(resp);
                log.Info("Cerrando petición...");
                peticion.Close();
                log.Info("Termina SolReportarHallazgoDemanda");

            }
            catch (Exception ex)
            {
                log.Error(ex.ToString());
            }
            return resp;
        }

        

        private static ConexCentralClient CreaReferenciaServicioDV(string ip)
        {
            
            EndpointIdentity spn = EndpointIdentity.CreateSpnIdentity("");
            Uri uri = new Uri(string.Format("http://{0}:{1}/OrqTiendaWcf", ip, "10002"));
            var address = new EndpointAddress(uri, spn);

            log.Info("Termina de crear la referencia a " + ip);


            return new ConexCentralClient("BasicHttpBinding_IConexCentral", address);
        }


        public string CrearCatalogoDVs()
        {
            try
            {
            Directory[] array_directory = {
                new Directory()
                {
                    path = "E:\\PuntoDeVenta\\Tienda\\AdministraCambaceo",
                    accesRead = true,
                    accesExecute = false,
                    appRoot = "Default.aspx"
                },
                new Directory()
                {
                    path = "E:\\PuntoDeVenta\\GerentePV",
                    accesRead = true,
                    accesExecute = false,
                    appRoot = "Default.html"
                }
            };

            CatalogoDVs catalogoDVs = new CatalogoDVs{
                directorys = array_directory 
            };
            log.Info( "Entregando el catalogo de DV " + JsonConvert.SerializeObject(catalogoDVs) );
            return JsonConvert.SerializeObject(catalogoDVs);
            }
            catch (Exception)
            {
                log.Info("No se pudo generar el catalogo de Directorio Virtuales");
                return "Ocurrio un error a la hora de generar el catalogo de directorios Virtuasles";
            }
        }

        //public string SolZip(string ip, string ruta)
        //{
            
        //}
    }



    public class CatalogoDVs
    {
        public Directory[] directorys { get; set; }
        
    }

    public class Directory
    {
        [JsonProperty]
        public string path { get; set; }
        [JsonProperty]
        public bool accesRead { get; set; }
        [JsonProperty]
        public bool accesExecute { get; set; }
        [JsonProperty]
        public string appRoot { get; set; }
    }
}