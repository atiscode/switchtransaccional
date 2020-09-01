using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AtisCode.Aplicacion
{
    public class dtoMundialMilesRet
    {
        public List<dtoCabeceraRet> Cabeceras { get; set; }
        public List<dtoDetalleRet> Detalles { get; set; }
        public dtoMundialMilesRet()
        {
            Cabeceras = new List<dtoCabeceraRet>();
            Detalles = new List<dtoDetalleRet>();
        }
    }
}