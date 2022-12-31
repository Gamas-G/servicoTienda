using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DMPMSDV
{
    class Jobs
    {
        public string Nombre { get; set; }
        public string Grupo { get; set; }
        public Jobs(string nombre, string grupo)
        {
            this.Nombre = nombre;
            this.Grupo = grupo;
        }
    }
}
