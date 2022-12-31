
namespace DMPMSDV
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Banco.PD3.Persistence;
    using Banco.PD3.Persistence.Entities;
    using System.Data.SqlClient;
    using Microsoft.Web.Administration;
    using System.Configuration;
    using System.Diagnostics;
    using global::DMPMSDV.model;
    using System.IO;
    using Encriptar;

    public class Monitoreo
    {
        PD3Connection conn1;
        string Qry = string.Empty;
        private static log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private string pathCatalogo = ConfigurationManager.AppSettings["catalogoDVs"];

        Seguridad seguridad = new Seguridad();
        //Zona de variables
        string jsonDVsEncript,
               jsonDVs;

        string path = ConfigurationManager.AppSettings["path"];
        string certificado = ConfigurationManager.AppSettings["certificado"];
        //string certificado = "baz-50b191cc-c36a-4e3b-83c7-470728efff5d.pd3"; //Producción
        //string certificado = "baz-52bd6396-0f8d-4188-959e-4f1083bfd836.pd3"; //Desarrollo

        public bool MonitorDV()
        {
            List<DirectorioVirtuale> direc;
            List<Application> vds2;
            List<Hallazgo> report = new List<Hallazgo>();
            Hallazgo hallazgo;
            DirectorioIIS dvIIS;
            DirectorioI direcIIS;
            List<DirectorioIIS> listDVIIS = new List<DirectorioIIS>(); ;
            string SitioWeb = "localhost";
            try
            {
                log.Info(string.Format("Inicia proceso de Monitoreo de Directorios Virtuales"));
                DateTime HApert = DateTime.ParseExact(Globales.HorarioApertura, "HH:mm", System.Globalization.CultureInfo.InvariantCulture);
                DateTime HCierre = DateTime.ParseExact(Globales.HorarioCierre, "HH:mm", System.Globalization.CultureInfo.InvariantCulture);
                DateTime HAct = DateTime.ParseExact(DateTime.Now.Hour.ToString("00") + ":" + DateTime.Now.Minute.ToString("00"), "HH:mm", System.Globalization.CultureInfo.InvariantCulture);
                log.Info(string.Format("HApert:" + HApert.ToString() + ", HCierre:" + HCierre.ToString() + ", HAct:" + HAct.ToString()));

                //VALIDANDO HORARIO DE TIENDA
                /*if( HAct <= HApert && HAct >= HCierre )
                {
                    log.Info( "HORARIO DE TIENDA CERRADA-SIN MONITOREO" );
                    return true;
                }*/



                if (HAct >= HApert && HAct <= HCierre) //No se realizara el monitoreo mientras estamos en jornada de trabajo
                {
                    log.Info("SUCURSAL EN HORARIO DE TRABAJO");
                    log.Info("MÁS INFORMACIÓN SOBRE LA ESTACION");
                    log.Info($"WS: {Globales.TipoWS}");
                    log.Info($"Workstation: {Globales.WorkStation}");

                    //OBTENIENDO LA DEZEREALIZACION DE NUESTRO CATALOGO
                    DirectorioVirtuale mio = new DirectorioVirtuale();
                    /*
                     * OBTENEMOS TODOS LOS DIRECTORIOS VIRTUALES
                     * DE NUESTRO CATALOGODVS ALMACENADOS EN UNA LISTA DE TIPO DIRECTORIOVIRTUALE
                     */
                    direc = mio.catalogo(Globales.catalogoDVDecrypt);
                    



                    //PROCEDEMOS A EXTRAER LOS DIRECTORIOS VIRTUALES DEL IIS
                    using (var iis = ServerManager.OpenRemote(SitioWeb))
                    {
                        //log.Info("Obteniendo directorios virtuales del IIS");
                        List<Site> sites = iis.Sites.ToList();//EXTRACCIÓN DE TODOS LOS SITES DEL IIS
                        //log.Info($"TOTAL DE SITES: {sites.Count}\n");


                        foreach (Site site in sites)
                        {
                            //log.Info("SITE NAME: " + site.Name);
                            dvIIS = new DirectorioIIS();
                            dvIIS.SiteWeb = site.Name;
                            //SE EXTRE LA LISTA DE DV PARA ITERAR SUS ATRIBUTOS DE LOS DVS DEL IIS
                            vds2 = site.Applications.ToList();
                            //log.Info(dvIIS.SiteWeb);
                            //log.Info("PRIMERA INFORMACIÓN");
                            for ( int i = 1; i < vds2.Count; i++ )
                            {
                                direcIIS = new DirectorioI();
                                //log.Info($"PATH: {vds2[i].Path.Trim('/')}");
                                //log.Info($"POOL: {vds2[i].ApplicationPoolName}");

                                direcIIS.NombreDV = ( string.IsNullOrEmpty( vds2[i].Path.Trim(new Char[] { '/', ' ' }) ) ) ? "" : vds2[i].Path.Trim('/');
                                direcIIS.PoolNameDV = ( string.IsNullOrEmpty( vds2[i].ApplicationPoolName.Trim() ) ) ? "" : vds2[i].ApplicationPoolName;

                                if (vds2[i].VirtualDirectories.Count <= 0)
                                {
                                    direcIIS.PathDv = "vacioantes";
                                    //dvIIS.directorio.Add(direcIIS);
                                    continue;
                                }

                                foreach (VirtualDirectory atributeDV in vds2[i].VirtualDirectories)
                                {
                                    //log.Info($"PHYSICALPATH {atributeDV.PhysicalPath}");
                                    direcIIS.PathDv = (string.IsNullOrEmpty(atributeDV.PhysicalPath.Trim())) ? "vacio" : atributeDV.PhysicalPath;
                                }
                                //log.Info("Despues de continue");
                                //log.Info(direcIIS.NombreDV);
                                //log.Info(direcIIS.PoolNameDV);
                                //log.Info(direcIIS.PathDv);
                                dvIIS.directorio.Add(direcIIS);
                            }
                            //log.Info("");




                            //foreach (Application DVs in site.Applications)
                            //{
                            //    if (DVs.VirtualDirectories.Count > 0)
                            //    {
                            //        log.Info("Path : " + DVs.Path);
                            //        log.Info("POOL : " + DVs.ApplicationPoolName);
                            //        //log.Info("ENABLEPROTOCOLOS : " + DVs.EnabledProtocols);
                            //        //log.Info("COUNT :" + DVs.VirtualDirectories.Count);
                            //        foreach (VirtualDirectory DV in DVs.VirtualDirectories)
                            //        {
                            //            //log.Info("DVUser: " + DV.UserName);
                            //            log.Info("DV: " + DV);
                            //            log.Info("Physicalpath :" + DV.PhysicalPath);
                            //            //log.Info("Path :" + DV.Path);
                            //        }
                            //        //foreach (VirtualDirectory directorio in DVs.VirtualDirectories)
                            //        //{
                            //        //    log.Info("DIRECTORIO : " + directorio);
                            //        //    log.Info("DIRECTORIO : " + directorio.UserName);
                            //        //    log.Info("ATRIBUTOS");
                            //        //    foreach (var item in directorio.Attributes)
                            //        //    {
                            //        //        log.Info("VALUES : Name :" + item.Name + " Value: " + item.Value);
                            //        //        log.Info("Schema : " + item.Schema.Name);
                            //        //    }
                            //        //}
                            //    }
                            //}
                            listDVIIS.Add(dvIIS);
                            //log.Info(listDVIIS.Count);
                        }
                    }//FINAL DE REPORTEADOR NUEVA IMPLEMENTACIÓN


                    //log.Info("ITERANDO LIST DE DV DEL IIS");
                    //foreach (DirectorioIIS item in listDVIIS)
                    //{
                    //    log.Info($"TOTAL; {listDVIIS.Count}");
                    //    log.Info($"SITEWEB : {item.SiteWeb}");
                    //    foreach (var dvs in item.directorio)
                    //    {
                    //        log.Info($"NOMBREDV: {dvs.NombreDV}");
                    //        log.Info($"PATHDV  : {dvs.PathDv}");
                    //        log.Info($"POOLNAME: {dvs.PoolNameDV}");
                    //    }
                    //    log.Info("");
                    //}


                    //INICIA BUSQUEDA DE DIRECTORIOS QUE NO SE ENCUENTRAN EN NUESTRO CATALOGODVS
                    //if( direc.Count != listDVIIS.Count)
                    //{
                    //    log.Info("LA CANTIDAD DE DIRECTORIOS VIRTUALES NO SON LAS MIMAS, SE PROCEDE A SINCRONIZAR EL CATALOGODVS");
                    //    if (!SincronizaCatalogoDVsRestauracion())
                    //    {
                    //        log.Info("Ocurrio un error a la hora de sincronizar por reporte");
                    //        return false;
                    //    }

                    //}
                    foreach (DirectorioVirtuale DVCat in direc)
                    {
                        if(listDVIIS.Find(x  => x.SiteWeb.Equals(DVCat.WebSite)))
                        {
                            log.Info("");
                        }
                        //foreach (DirectorioIIS DVIIS in listDVIIS)
                        //{

                        //}
                    }


                    //REPORTADEARO DE HALLAZGOS CON DEPENDENCIA DE BASE DE DATOS
                        //DMPMicroservicio.csMsMetodos dllMs = new DMPMicroservicio.csMsMetodos();
                        //conn1 = new PD3Connection();
                        //conn1 = Certificater.GetPD3Connection(path, certificado);
                        //SqlPersister persistence = new SqlPersister(conn1);
                        //SqlDataReader reader = null;
                        //Qry = "exec spConDMPDirVirtualMtr";
                        //reader = persistence.ScriptToDataReader(Qry);

                        //if (reader.HasRows)
                        //{
                        //    string SitioWeb = "localhost";
                        //    using (var iis = ServerManager.OpenRemote(SitioWeb))
                        //    {
                        //        List<Site> sites = iis.Sites.ToList();

                        //        while (reader.Read())
                        //        {
                        //            int veces = 0;
                        //            bool existedv = false;
                        //            foreach (Site sib in sites)
                        //            {
                        //                if (sites[veces].Name == reader[1].ToString().Trim())
                        //                {
                        //                    List<Microsoft.Web.Administration.Application> vds2 = sites[veces].Applications.ToList();
                        //                    foreach (Microsoft.Web.Administration.Application dv2 in vds2)
                        //                    {
                        //                        if (dv2.Path == ("/" + reader[6].ToString().Trim()))
                        //                        {
                        //                            existedv = true;
                        //                            break;
                        //                        }
                        //                    }
                        //                }
                        //                veces += 1;
                        //                if (existedv)
                        //                {
                        //                    veces = 0;
                        //                    break;
                        //                }
                        //            }

                        //            if (!existedv)
                        //            {
                        //                log.Info(string.Format("Se genera Hallazgo"));

                        //                //log.Info("DESARROLLO- " + reader[5].ToString().Trim());
                        //                //log.Info("DESARROLLO- " + reader[0].ToString().Trim());

                        //                string MsjCritico = reader[5].ToString().Trim() == "1" ? "CRITICO|" + reader[0].ToString().Trim() + "|" : string.Empty;

                        //                //log.Info("DESARROLLO- "+reader[7].ToString().Trim());
                        //                //log.Info("DESARROLLO- "+reader[6].ToString().Trim());
                        //                //log.Info("DESARROLLO- "+reader[1].ToString().Trim());

                        //                dllMs.GeneraHallazgo(Convert.ToInt32(reader[7].ToString().Trim()), MsjCritico + "No se encontro el Directorio Virtual " + reader[6].ToString().Trim() + " del sitio " + reader[1].ToString().Trim(), "DV", 2);

                        //                log.Info("Mensaje enviando a dll... " + Convert.ToInt32(reader[7].ToString().Trim()) +  MsjCritico + "No se encontro el Directorio Virtual " + reader[6].ToString().Trim() + " del sitio " + reader[1].ToString().Trim()+ "DV"+ 2);

                        //            }

                        //        }
                        //    }
                        //}
                        //reader.Close();

                        //dllMs.ControlCheck("DV", Globales.WorkStation, "DV");
                        log.Info(string.Format("Termina correctamente proceso de Monitoreo de Directorios Virtuales"));
                    return true;
                }
                else
                {
                    log.Info(string.Format("Horario de Tienda Cerrada-Sin Monitoreo"));
                    return true;
                }

            }
            catch (Exception ex)
            {
                log.Error(string.Format("Error en MonitorDV: {0}", ex.Message));
                return false;
            }
        }

        //Carga la información que tendra nuestro WCF
        public void CargaConfiguracion()
        {
            try
            {
                log.Info(string.Format("Inicia carga de Configuración"));

                DMPMicroservicio.csMsMetodos dllMs = new DMPMicroservicio.csMsMetodos();

                dllMs.CargaConfiguracion("DV");
                Globales.TipoMtrIni      = dllMs.TipoMtrIni;
                //Globales.HoraIniMtr = dllMs.HoraIniMtr;
                //Globales.IntervaloMtr    = dllMs.IntervaloMtr;
                Globales.HoraIniMtr      = ConfigurationManager.AppSettings["horaInicio"];//HORA EN LA QUE ABRE TIENDA
                Globales.IntervaloMtr    = Convert.ToInt32( ConfigurationManager.AppSettings["intervaloMonitoreo"] );//INTERVALOS DE MONITOREO
                Globales.IntervMinMtr    = dllMs.IntervMinMtr;
                Globales.ToleranciaMtr   = dllMs.ToleranciaMtr;//TOLERANCIA -> LOS MINUTOS MINIMOS TRANSCURRIDOS DESPUES DE LA HORA DE APERTURA.
                Globales.HorarioApertura = dllMs.HorarioApertura;
                //Globales.HorarioCierre   = dllMs.HorarioCierre;            
                Globales.HorarioCierre   = ConfigurationManager.AppSettings["horacierre"];            
                Globales.ipNT            = dllMs.ObtenerIP();//OBTENCION DE LA IP DE LA MAQUINA

                log.Info("La IP obtenida es: " + Globales.ipNT);

                log.Info(string.Format("Termina carga de Configuración"));
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Error en CargaConfiguracion: {0}", ex.Message));
            }
        }

        public void AgendarTarea(int Tipo, string Tarea, string Gatillo,string HoraAlter)
        {
            TareasProgramadas TareasP = new TareasProgramadas();            
            TareasP.Monitoreo(Tipo, Tarea, Gatillo, HoraAlter);
        }        

        public void InicioAlterno()
        {            
            try
            {
                log.Info(string.Format("Inicia proceso InicioAlterno"));

                string HoraProg = string.Empty;
                DMPMicroservicio.csMsMetodos dllMs = new DMPMicroservicio.csMsMetodos();
                HoraProg = dllMs.InicioAlterno(Globales.HoraIniMtr, Globales.IntervaloMtr, "DV");

                log.Info(string.Format("Se programara tarea de Monitoreo Inicial Alterno"));
                TareasProgramadas TareasP = new TareasProgramadas();
                TareasP.Monitoreo(3, "MtrIniAlter", "triggerMtrAlter", HoraProg);

                log.Info(string.Format("Termina proceso InicioAlterno"));
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Error en InicioAlterno: {0}", ex.Message));
            }
        }


        public void SincronizaCatalogoDVs()
        {
            /*
             * SI NO EXISTE EL CATALOGO, PROCEDEMOS A OBTENERLO DE CENTRAL
             */
            log.Info("Validando existencia de catalogoDvs");
            try
            {
                if (!File.Exists(pathCatalogo))
                {
                    /*CONSUMIMOS EL SERVICIO WCF DE CENTRAL*/
                    log.Info("CatalogoDVs no existe, iniciando conexión con gdtSyncDvTienda");

                    DMPMSDVC.centraloGer.ConexSucClient cn = new DMPMSDVC.centraloGer.ConexSucClient();
                    //DMPMSDV.centraloGer.ConexSucClient cn = new DMPMSDVC.centraloGer.ConexSucClient();
                    jsonDVs = cn.SincronizarDvTienda();//Consultamos información con central

                    Globales.catalogoDVDecrypt = jsonDVs;//Guardamos la información en Globales para su uso directo

                    jsonDVsEncript = seguridad.Encriptar(jsonDVs);//Encriptando la información de catalogoDVs
                    File.WriteAllText(pathCatalogo, jsonDVsEncript);//Creando el archivo, con la incriptación
                    log.Info("CatalogoDVs encriptado correctamente");
                }
                else
                {
                    log.Info("El catalogo existe, DESENCRIPTANDO y almacenando información para su trabajo");
                    string contentFile = ReadCatalogoDVs(pathCatalogo);
                    Globales.catalogoDVDecrypt = seguridad.Desencriptar(contentFile);
                    log.Info("Desencriptación exitosa");

                }
            }
            catch (Exception ex)
            {
                log.Info(
                         "Metodo validaCatalogoDVs\n" +
                         "Ocurrio un error al sincronizar catalogoDVs con central: \n" +
                         ex.Message
                        );
            }
        }

        public bool SincronizaCatalogoDVsRestauracion()
        {
            /*
             * SI NO EXISTE EL CATALOGO, PROCEDEMOS A OBTENERLO DE CENTRAL
             */
            log.Info("Validando existencia de catalogoDvs por Restauración");
            try
            {
                if (File.Exists(pathCatalogo))
                {
                    File.Delete(pathCatalogo);
                }
                    /*CONSUMIMOS EL SERVICIO WCF DE CENTRAL*/
                    log.Info("Sincronizando CatalogoDVs por restauracion");

                    DMPMSDVC.centraloGer.ConexSucClient cn = new DMPMSDVC.centraloGer.ConexSucClient();
                    //DMPMSDV.centraloGer.ConexSucClient cn = new DMPMSDVC.centraloGer.ConexSucClient();
                    jsonDVs = cn.SincronizarDvTienda();//Consultamos información con central

                    Globales.catalogoDVDecrypt = jsonDVs;//Guardamos la información en Globales para su uso directo

                    jsonDVsEncript = seguridad.Encriptar(jsonDVs);//Encriptando la información de catalogoDVs
                    File.WriteAllText(pathCatalogo, jsonDVsEncript);//Creando el archivo, con la incriptación
                    log.Info("CatalogoDVs encriptado correctamente");
                return true;
            }
            catch (Exception ex)
            {
                log.Info(
                         "Metodo validaCatalogoDVsRestauracion\n" +
                         "Ocurrio un error al sincronizar catalogoDVs por restauración con central: \n" +
                         ex.Message
                        );
                return false;
            }
        }


        public string ReadCatalogoDVs(string path)
        {
            string fileInfo = string.Empty;
            try
            {
                fileInfo = File.ReadAllText(path);
            }
            catch (Exception msj)
            {
                log.Info("Ocurrio un erro al leer catalogoDVs\n Exception: " + msj.Message);
            }
            return fileInfo;
        }
    }
}
