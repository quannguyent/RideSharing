using System;using Thinktecture;using Thinktecture;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace RideSharing.Models
{
    public partial class DataContext : DbContext
    {
        public virtual DbSet<BusStopDAO> BusStop { get; set; }
        public virtual DbSet<CityFreighterDAO> CityFreighter { get; set; }
        public virtual DbSet<CustomerDAO> Customer { get; set; }
        public virtual DbSet<DeliveryOrderDAO> DeliveryOrder { get; set; }
        public virtual DbSet<DeliveryRouteDAO> DeliveryRoute { get; set; }
        public virtual DbSet<DeliveryTripDAO> DeliveryTrip { get; set; }
        public virtual DbSet<NodeDAO> Node { get; set; }
        public virtual DbSet<SystemConfigDAO> SystemConfig { get; set; }

        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. See http://go.microsoft.com/fwlink/?LinkId=723263 for guidance on storing connection strings.
                optionsBuilder.UseSqlServer("data source=LAPTOP-DK17N0RS\\MSSQLSERVER2019;initial catalog=RideSharing;persist security info=True;Trusted_Connection=True;multipleactiveresultsets=True;");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ConfigureTempTable<long>();modelBuilder.ConfigureTempTable<Guid>();modelBuilder.Entity<BusStopDAO>(entity =>
            {
                entity.Property(e => e.CreatedAt).HasColumnType("datetime");

                entity.Property(e => e.DeletedAt).HasColumnType("datetime");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.UpdatedAt).HasColumnType("datetime");

                entity.HasOne(d => d.Node)
                    .WithMany(p => p.BusStops)
                    .HasForeignKey(d => d.NodeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_BusStop_Node");
            });

            modelBuilder.Entity<CityFreighterDAO>(entity =>
            {
                entity.Property(e => e.Capacity).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.CreatedAt).HasColumnType("datetime");

                entity.Property(e => e.DeletedAt).HasColumnType("datetime");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.UpdatedAt).HasColumnType("datetime");

                entity.HasOne(d => d.Node)
                    .WithMany(p => p.CityFreighters)
                    .HasForeignKey(d => d.NodeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_CityFreighter_Node");
            });

            modelBuilder.Entity<CustomerDAO>(entity =>
            {
                entity.Property(e => e.Code)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.CreatedAt).HasColumnType("datetime");

                entity.Property(e => e.DeletedAt).HasColumnType("datetime");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.UpdatedAt).HasColumnType("datetime");

                entity.HasOne(d => d.Node)
                    .WithMany(p => p.Customers)
                    .HasForeignKey(d => d.NodeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Customer_Node");
            });

            modelBuilder.Entity<DeliveryOrderDAO>(entity =>
            {
                entity.Property(e => e.Code)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.CreatedAt).HasColumnType("datetime");

                entity.Property(e => e.DeletedAt).HasColumnType("datetime");

                entity.Property(e => e.UpdatedAt).HasColumnType("datetime");

                entity.Property(e => e.Weight).HasColumnType("decimal(18, 4)");

                entity.HasOne(d => d.Customer)
                    .WithMany(p => p.DeliveryOrders)
                    .HasForeignKey(d => d.CustomerId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_DeliveryOrder_Customer");
            });

            modelBuilder.Entity<DeliveryRouteDAO>(entity =>
            {
                entity.Property(e => e.CreatedAt).HasColumnType("datetime");

                entity.Property(e => e.DeletedAt).HasColumnType("datetime");

                entity.Property(e => e.Path)
                    .IsRequired()
                    .HasMaxLength(500);

                entity.Property(e => e.TotalEmptyRunDistance).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.TotalTravelDistance).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.UpdatedAt).HasColumnType("datetime");

                entity.HasOne(d => d.CityFreighter)
                    .WithMany(p => p.DeliveryRoutes)
                    .HasForeignKey(d => d.CityFreighterId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_DeliveryRoute_CityFreighter");
            });

            modelBuilder.Entity<DeliveryTripDAO>(entity =>
            {
                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.CreatedAt).HasColumnType("datetime");

                entity.Property(e => e.DeletedAt).HasColumnType("datetime");

                entity.Property(e => e.Path)
                    .IsRequired()
                    .HasMaxLength(500);

                entity.Property(e => e.TravelDistance).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.UpdatedAt).HasColumnType("datetime");

                entity.HasOne(d => d.BusStop)
                    .WithMany(p => p.DeliveryTrips)
                    .HasForeignKey(d => d.BusStopId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_DeliveryTrip_BusStop");

                entity.HasOne(d => d.CityFreighter)
                    .WithMany(p => p.DeliveryTrips)
                    .HasForeignKey(d => d.CityFreighterId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_DeliveryTrip_CityFreighter");
            });

            modelBuilder.Entity<NodeDAO>(entity =>
            {
                entity.Property(e => e.Code)
                    .IsRequired()
                    .HasMaxLength(4000);

                entity.Property(e => e.Latitude).HasColumnType("decimal(18, 10)");

                entity.Property(e => e.Longtitude).HasColumnType("decimal(18, 10)");
            });

            modelBuilder.Entity<SystemConfigDAO>(entity =>
            {
                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.Code)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.CreatedAt).HasColumnType("datetime");

                entity.Property(e => e.DeletedAt).HasColumnType("datetime");

                entity.Property(e => e.DeliveryRadius).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.FreighterQuotientCost).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.UpdatedAt).HasColumnType("datetime");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
