﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AtisCode.Aplicacion
{
    public class Respuesta
    {
        public string mensaje { get; set; }
        public string codigoRetorno { get; set; }
        public string estado { get; set; }
        public string numeroDocumento { get; set; }
    }

    public class Respuesta2
    {
        public string mensaje { get; set; }
        public string codigoRetorno { get; set; }
        public string estado { get; set; }
        public string numeroDocumento { get; set; }
        public string secuencial { get; set; }
    }
}
