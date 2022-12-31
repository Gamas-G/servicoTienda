using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DMPMSDV.model
{
    class Hallazgos
    {
        //[JsonProperty("hallazgos")]
        public Hallazgo Hallazgo { get; set; }
    }

    public class Hallazgo
    {
        [JsonProperty("dvId")]
        public int DvId { get; set; }

        [JsonProperty("sucursal")]
        public int Sucursal { get; set; }
        
        [JsonProperty("workstation")]
        public string Workstation { get; set; }
        
        [JsonProperty("fechaReporte")]
        public string FechaReporte { get; set; }
        
        [JsonProperty("detalle")]
        public string Detalle { get; set; }
        
        [JsonProperty("estatusId")]
        public int EstatusId { get; set; }

        [JsonProperty("descripcionDV")]
        public string DescripcionDV { get; set; }

        [JsonProperty("descripcionSitioWeb")]
        public string DescripcionSitioWeb { get; set; }

        [JsonProperty("applicacionPoolDV")]
        public string ApplicationPoolDV { get; set; }
    }
}
