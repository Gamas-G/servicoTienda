﻿//------------------------------------------------------------------------------
// <auto-generated>
//     Este código fue generado por una herramienta.
//     Versión de runtime:4.0.30319.42000
//
//     Los cambios en este archivo podrían causar un comportamiento incorrecto y se perderán si
//     se vuelve a generar el código.
// </auto-generated>
//------------------------------------------------------------------------------

namespace OrquestadorCentralWs.ServiceReference1 {
    
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ServiceModel.ServiceContractAttribute(ConfigurationName="ServiceReference1.IWcfDMPMonitorDV")]
    public interface IWcfDMPMonitorDV {
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IWcfDMPMonitorDV/MonitoreoDVPorSolicitud", ReplyAction="http://tempuri.org/IWcfDMPMonitorDV/MonitoreoDVPorSolicitudResponse")]
        bool MonitoreoDVPorSolicitud();
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IWcfDMPMonitorDV/RestauracionDV", ReplyAction="http://tempuri.org/IWcfDMPMonitorDV/RestauracionDVResponse")]
        string RestauracionDV(string nombreDV);
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public interface IWcfDMPMonitorDVChannel : OrquestadorCentralWs.ServiceReference1.IWcfDMPMonitorDV, System.ServiceModel.IClientChannel {
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public partial class WcfDMPMonitorDVClient : System.ServiceModel.ClientBase<OrquestadorCentralWs.ServiceReference1.IWcfDMPMonitorDV>, OrquestadorCentralWs.ServiceReference1.IWcfDMPMonitorDV {
        
        public WcfDMPMonitorDVClient() {
        }
        
        public WcfDMPMonitorDVClient(string endpointConfigurationName) : 
                base(endpointConfigurationName) {
        }
        
        public WcfDMPMonitorDVClient(string endpointConfigurationName, string remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public WcfDMPMonitorDVClient(string endpointConfigurationName, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public WcfDMPMonitorDVClient(System.ServiceModel.Channels.Binding binding, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(binding, remoteAddress) {
        }
        
        public bool MonitoreoDVPorSolicitud() {
            return base.Channel.MonitoreoDVPorSolicitud();
        }
        
        public string RestauracionDV(string nombreDV) {
            return base.Channel.RestauracionDV(nombreDV);
        }
    }
}
