
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data.AppDbContext;
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Customer> Customers { get; set; }
        public DbSet<CustomerFile> CustomerFiles { get; set; }
        public DbSet<Loan> Loans { get; set; }
        public DbSet<AdminUser> AdminUsers { get; set; }
        public DbSet<RepaymentSchedule> RepaymentSchedules { get; set; }
        public DbSet<DeviceStatus> DeviceStatuses { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Customer -> DeviceStatus (1:1)
            modelBuilder.Entity<Customer>()
                .HasOne(c => c.DeviceStatus)
                .WithOne(d => d.Customer)
                .HasForeignKey<DeviceStatus>(d => d.CustomerId);
            // Customer -> FIle (1:1)
            modelBuilder.Entity<Customer>()
                .HasOne(c => c.CustomerFile)
                .WithOne(d => d.Customer)
                .HasForeignKey<CustomerFile>(d => d.CustomerId);

            // Loan -> Customer (Many:1)
            modelBuilder.Entity<Loan>()
                .HasOne(l => l.Customer)
                .WithMany(c => c.Loans)
                .HasForeignKey(l => l.CustomerId);

            // RepaymentSchedule -> Loan (Many:1)
            modelBuilder.Entity<RepaymentSchedule>()
                .HasOne(r => r.Loan)
                .WithMany(l => l.RepaymentSchedules)
                .HasForeignKey(r => r.LoanId);

            // Ensure IMEI is unique
            modelBuilder.Entity<Customer>()
                .HasIndex(c => c.IMEI)
                .IsUnique();
        }
    }
