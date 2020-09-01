using AtisCode.Aplicacion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace App_Mundial_Miles.Models
{
    public class Documento
    {
        public dtoCabecera Cabeceras { get; set; }
        public List<dtoDetalle> Detalles { get; set; }
        public Documento()
        {
            Detalles = new List<dtoDetalle>();
        }
    }
}