//------------------------------------------------------------------------------
// <auto-generated>
//     Este código se generó a partir de una plantilla.
//
//     Los cambios manuales en este archivo pueden causar un comportamiento inesperado de la aplicación.
//     Los cambios manuales en este archivo se sobrescribirán si se regenera el código.
// </auto-generated>
//------------------------------------------------------------------------------

namespace AtisCode.Aplicacion.Model.db_Safi
{
    using System;
    using System.Collections.Generic;
    
    public partial class tfacturas
    {
        public int id { get; set; }
        public string tipo { get; set; }
        public string factura { get; set; }
        public Nullable<int> Estado { get; set; }
        public string AutorizacionSRI { get; set; }
        public string ClaveAcceso { get; set; }
        public Nullable<System.DateTime> FechaAutorizacion { get; set; }
        public Nullable<int> idFactura { get; set; }
        public string DescripcionError { get; set; }
        public string CodigoError { get; set; }
        public string CodigoFormaPago { get; set; }
        public string Plazo { get; set; }
        public string secuencialerp { get; set; }
        public string numdocumento { get; set; }
        public string email_cliente { get; set; }
    }
}
