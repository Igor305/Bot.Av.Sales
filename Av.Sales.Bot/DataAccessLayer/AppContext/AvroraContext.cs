using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

#nullable disable

namespace DataAccessLayer.AppContext
{
    public partial class AvroraContext : DbContext
    {
        public AvroraContext()
        {
        }

        public AvroraContext(DbContextOptions<AvroraContext> options)
            : base(options)
        {
        }

        public virtual DbSet<ItExecutionPlanShop> ItExecutionPlanShops { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
                optionsBuilder.UseSqlServer("Data Source=sql03;Initial Catalog=Avrora;Persist Security Info=True;User ID=j-PlanShops-Reader;Password=AE97rX3j5n");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("j-PlanShops-Reader")
                .HasAnnotation("Relational:Collation", "Cyrillic_General_CI_AS");

            modelBuilder.Entity<ItExecutionPlanShop>(entity =>
            {
                entity.HasKey(e => e.StockId)
                    .HasName("PK__it_Execu__2C83A9E279502029");

                entity.ToTable("it_ExecutionPlanShops", "dbo");

                entity.Property(e => e.StockId)
                    .ValueGeneratedNever()
                    .HasColumnName("StockID");

                entity.Property(e => e.Dates).HasColumnType("smalldatetime");

                entity.Property(e => e.FactDay).HasColumnType("numeric(21, 9)");

                entity.Property(e => e.FactMonth).HasColumnType("numeric(21, 9)");

                entity.Property(e => e.PercentForDay).HasColumnType("numeric(21, 9)");

                entity.Property(e => e.PercentForMonth).HasColumnType("numeric(21, 9)");

                entity.Property(e => e.PercentForecast).HasColumnType("numeric(21, 9)");

                entity.Property(e => e.PlanDay).HasColumnType("numeric(21, 9)");

                entity.Property(e => e.PlanMonth).HasColumnType("numeric(21, 9)");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
