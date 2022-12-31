sc create WSDMPSW binpath= "E:\Users\b335913\Desktop\Monitoreo\MONITOREO BUENO\SERVICIOS MONITOREO\Monitoreo\Microservicios\DMPMSSW\DMPMSSW\bin\Debug\DMPMSSW.exe" DisplayName= "WSDMPSW" type= own start= auto
sc description WSDMPSW "Microservicio Servicios Windows"
net start WSDMPSW
