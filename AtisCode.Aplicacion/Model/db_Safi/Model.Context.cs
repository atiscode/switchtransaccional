﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace AtisCode.Aplicacion.Model.db_Safi
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class SafiEntities : DbContext
    {
        public SafiEntities()
            : base("name=SafiEntities")
        {
        }

        public SafiEntities(string connection)
    : base("name="+ connection)
        {
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<FACHIS> FACHIS { get; set; }
        public virtual DbSet<CXCDIR> CXCDIR { get; set; }
        public virtual DbSet<tfacturas> tfacturas { get; set; }
        public virtual DbSet<tdocumentoscontador> tdocumentoscontador { get; set; }
        public virtual DbSet<CXPDIR> CXPDIR { get; set; }
    }
}
