//------------------------------------------------------------------------------
// <auto-generated>
//     Este código se generó a partir de una plantilla.
//
//     Los cambios manuales en este archivo pueden causar un comportamiento inesperado de la aplicación.
//     Los cambios manuales en este archivo se sobrescribirán si se regenera el código.
// </auto-generated>
//------------------------------------------------------------------------------

namespace AtisCode.Aplicacion.Model.db_Local
{
    using System;
    using System.Collections.Generic;
    
    public partial class AtisQueueTransactions
    {
        public long ID { get; set; }
        public string ObjectRequest { get; set; }
        public string Sequential { get; set; }
        public string Channel { get; set; }
        public System.DateTime DateStart { get; set; }
        public Nullable<System.DateTime> DateEnd { get; set; }
        public string Status { get; set; }
        public bool Ready { get; set; }
        public string ExceptionsService { get; set; }
        public string TypeDocument { get; set; }
        public string RequestXML { get; set; }
    }
}
