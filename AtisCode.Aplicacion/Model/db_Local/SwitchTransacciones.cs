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
    
    public partial class SwitchTransacciones
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public SwitchTransacciones()
        {
            this.EmpresaSwitchTransacciones = new HashSet<EmpresaSwitchTransacciones>();
        }
    
        public long Id { get; set; }
        public int Estado { get; set; }
        public System.DateTime Fecha { get; set; }
        public string Segmento { get; set; }
        public string PtoEmi { get; set; }
        public Nullable<int> Ambiente { get; set; }
        public Nullable<int> Iva { get; set; }
        public string CodProducto { get; set; }
        public string NombProducto { get; set; }
        public string CategoriaCliente { get; set; }
        public string CuentaContable { get; set; }
        public Nullable<int> GrupoCredito { get; set; }
        public Nullable<int> DocumentoElectronico { get; set; }
        public Nullable<int> Relacionado { get; set; }
        public string VendedorSeccion { get; set; }
        public string ListaPrecioContado { get; set; }
        public string ListaPrecioCredito { get; set; }
        public Nullable<int> LimiteCredito { get; set; }
        public string Uge { get; set; }
        public string Bodega { get; set; }
        public string Tipo { get; set; }
        public string Status { get; set; }
        public Nullable<System.DateTime> StartControlDayTransactions { get; set; }
        public Nullable<System.DateTime> FinishControlDayTransactions { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<EmpresaSwitchTransacciones> EmpresaSwitchTransacciones { get; set; }
    }
}
