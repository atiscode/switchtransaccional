using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace App_Mundial_Miles.Models
{
    public partial class DocumentoGenerado
    {
        public string Numero { get; set; }
        public string Estado { get; set; }
        public string Descripcion { get; set; }
        public string ClaveAcceso { get; set; }
    }
}