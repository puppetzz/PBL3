using Microsoft.EntityFrameworkCore;
using PBL3.Models;

namespace PBL3.Data {
    public class ShopGuitarContext : DbContext {
        public ShopGuitarContext(DbContextOptions<ShopGuitarContext> options) : base(options) {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder) {
            modelBuilder.Entity<Employee>()
                .HasOne(e => e.Manager)
                .WithMany()
                .HasForeignKey(e => e.ManagerId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<ReceiptCommodity>().HasKey(rc => new {
                rc.ReceiptId,
                rc.CommodityId
            });
        }

        public DbSet<Account> Accounts { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<Salaries> Salaries { get; set; }
        public DbSet<Titles> Titles { get; set; }
        public DbSet<Receipt> Receipts { get; set; }
        public DbSet<ReceiptCommodity> ReceiptCommodities { get; set; }
        public DbSet<Commodity> Commodities { get; set; }
        public DbSet<Contact> Contacts { get; set; }
    }
}
