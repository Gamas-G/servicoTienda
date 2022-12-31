sc create WSDMPLR binpath= "C:\Vrael\Monitoreo\Microservicios\DMPMSLR\DMPMSLR\DMPMSLR\bin\Debug\DMPMSLR.exe" DisplayName= "DMP Monitoreo Llaves de Registro" type= own start= auto
sc description WSDMPLR "Microservicio Llaves de Registro"
net start WSDMPLR
