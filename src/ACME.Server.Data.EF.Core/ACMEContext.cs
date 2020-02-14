using Microsoft.EntityFrameworkCore;
using PeculiarVentures.ACME.Server.Data.EF.Core.Models;

namespace PeculiarVentures.ACME.Server.Data.EF.Core
{
    public class AcmeContext : DbContext
    {
        public AcmeContext(DbContextOptions<AcmeContext> options)
            : base(options)
        {
            Database.EnsureCreated();
        }

        public DbSet<Account> Accounts { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<Authorization> Authorizations { get; set; }
        public DbSet<Challenge> Challenges { get; set; }
        public DbSet<Error> Errors { get; set; }
        public DbSet<OrderAuthorization> OrderAuthorization { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Authorization>()
               .Property(o => o.AccountId)
               .IsRequired();

            modelBuilder.Entity<Authorization>()
                .HasOne(o => o.Account)
                .WithMany(o => o.Authorizations)
                .HasForeignKey(o => o.AccountId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
