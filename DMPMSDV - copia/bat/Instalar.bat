sc create WSDMPDV binpath= "C:\Vrael\Monitoreo\Microservicios\DMPMSDV\DMPMSDV\bin\Debug\DMPMSDV.exe" DisplayName= "DMP Monitoreo Directorios Virtuales" type= own start= auto
sc description WSDMPDV "Microservicio Directorios Virtuales"
net start WSDMPDV
