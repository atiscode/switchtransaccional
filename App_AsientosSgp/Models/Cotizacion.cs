using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace App_Mundial_Miles.Models
{
    public class Cotizacion
    {
        public string Cliente { get; set; }
        public string Identificacion { get; set; }
        public string MailCliente { get; set; }
        public string Direccion { get; set; }
        public string Detalle { get; set; }
        public string Cantidad { get; set; }
        public string PrecioUnitario { get; set; }
        public string SubTotal { get; set; }
        public string Descuento { get; set; }
        public string Total { get; set; }
        public string NumeroPedido { get; set; }
        public string Comentario1 { get; set; }
        public string Comentario2 { get; set; }
        public string NumeroOP { get; set; }
        public string Bodega { get; set; }
        public string ProductoServicio { get; set; }
        public string Secuencial { get; set; }

    }
}