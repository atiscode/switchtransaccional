//------------------------------------------------------------------------------
// <auto-generated>
//     Este código se generó a partir de una plantilla.
//
//     Los cambios manuales en este archivo pueden causar un comportamiento inesperado de la aplicación.
//     Los cambios manuales en este archivo se sobrescribirán si se regenera el código.
// </auto-generated>
//------------------------------------------------------------------------------

namespace AtisCode.Aplicacion.Model.db_SafiParametros
{
    using System;
    using System.Collections.Generic;
    
    public partial class ComprobanteMasivoHistorial
    {
        public int ComprobanteId { get; set; }
        public string Xml { get; set; }
        public int TipoComprobante { get; set; }
        public int CodigoRelacionId { get; set; }
        public int CodigoRelacionIdReal { get; set; }
        public int EstadoComprobanteId { get; set; }
        public System.DateTime FechaCreacion { get; set; }
        public Nullable<System.DateTime> FechaActualizacion { get; set; }
        public int UserId { get; set; }
        public string SeverBdd { get; set; }
        public string Bdd { get; set; }
        public string UsuarioBdd { get; set; }
        public string PasswordBdd { get; set; }
        public string Ruc { get; set; }
        public string MensajeProceso { get; set; }
        public Nullable<int> Ano { get; set; }
    }
}
