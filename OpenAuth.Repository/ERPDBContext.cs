using Microsoft.EntityFrameworkCore;
using OpenAuth.Repository.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.Repository
{
    public class ERPDBContext:DbContext
    {
        public ERPDBContext(DbContextOptions<ERPDBContext> options)
           : base(options)
        { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TF_BOM>()//.ToTable("TF_BOM");
                .HasKey(c => new { c.BOM_NO });
            modelBuilder.Entity<MF_BOM>().ToTable("MF_BOM")
              .HasKey(c => new { c.BOM_NO });
        }

        public virtual DbSet<TF_BOM> TF_BOMs { get; set; }
        public virtual DbSet<MF_BOM> MF_BOMs { get; set; }

    }
}
