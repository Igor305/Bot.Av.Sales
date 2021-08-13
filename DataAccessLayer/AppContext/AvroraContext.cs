using Microsoft.EntityFrameworkCore;

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
        public virtual DbSet<ItPlanSaleStockOnDate> ItPlanSaleStockOnDates { get; set; }
        public virtual DbSet<ItPlanSaleStockOnDateD> ItPlanSaleStockOnDateDs { get; set; }
        public virtual DbSet<SalesByCategoryManager> SalesByCategoryManagers { get; set; }

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

            modelBuilder.Entity<ItPlanSaleStockOnDate>(entity =>
            {
                entity.HasKey(e => e.ChId)
                    .HasName("PK__it_PlanS__AF02F0B882429C72");

                entity.ToTable("it_PlanSaleStockOnDate", "dbo");

                entity.Property(e => e.ChId)
                    .ValueGeneratedNever()
                    .HasColumnName("ChID");

                entity.Property(e => e.CreateDate).HasColumnType("smalldatetime");

                entity.Property(e => e.DocDate).HasColumnType("smalldatetime");
            });

            modelBuilder.Entity<ItPlanSaleStockOnDateD>(entity =>
            {
                entity.HasKey(e => new { e.ChId, e.StockId })
                    .HasName("PK__it_PlanS__9DCACA26A40BD7BB");

                entity.ToTable("it_PlanSaleStockOnDateD", "dbo");

                entity.Property(e => e.ChId).HasColumnName("ChID");

                entity.Property(e => e.StockId).HasColumnName("StockID");

                entity.Property(e => e.PlanSum).HasColumnType("numeric(21, 9)");

                entity.HasOne(d => d.Ch)
                    .WithMany(p => p.ItPlanSaleStockOnDateDs)
                    .HasForeignKey(d => d.ChId)
                    .HasConstraintName("FK__it_PlanSaleStockOnDateD_Sta__ChID");
            });

            modelBuilder.Entity<SalesByCategoryManager>(entity =>
            {
                entity.HasNoKey();

                entity.ToTable("SalesByCategoryManager", "dashboard");

                entity.Property(e => e.CategoryManagerId).HasColumnName("CategoryManagerID");

                entity.Property(e => e.CategoryManagerName)
                    .HasMaxLength(250)
                    .IsUnicode(false);

                entity.Property(e => e.Date).HasColumnType("smalldatetime");

                entity.Property(e => e.SalesByCategoryManager1)
                    .HasColumnType("numeric(21, 9)")
                    .HasColumnName("SalesByCategoryManager");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
