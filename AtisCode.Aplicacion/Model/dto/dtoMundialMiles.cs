using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AtisCode.Aplicacion
{
    public class dtoMundialMiles
    {
        public List<dtoCabecera> Cabeceras { get; set; }
        public List<dtoDetalle> Detalles { get; set; }
        public dtoMundialMiles()
        {
            Cabeceras = new List<dtoCabecera>();
            Detalles = new List<dtoDetalle>();
        }
    }
}