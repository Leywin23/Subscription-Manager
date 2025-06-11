using Microsoft.EntityFrameworkCore;
using Subscription_Manager.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Subscription_Manager.Models;

namespace Subscription_Manager.Data
{
    public class AppDbContext: IdentityDbContext<AppUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options):base(options){}

        public DbSet<Subscription> Subscriptions {  get; set; }
        public DbSet<UserSubscription> UserSubscriptions { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            SeedRoles(builder);
            ConfigureUserSubscription(builder);
        }
        private void SeedRoles(ModelBuilder builder)
        {
            List<IdentityRole> roles = new List<IdentityRole>()
            {
            new IdentityRole
            {
                Id = "1",
                Name = "Admin",
                NormalizedName = "ADMIN",
                ConcurrencyStamp = "1"
            },
            new IdentityRole
            {
                Id = "2",
                Name = "User",
                NormalizedName = "USER",
                ConcurrencyStamp = "2"
            }
            };
                builder.Entity<IdentityRole>().HasData(roles);
        }
        private void ConfigureUserSubscription(ModelBuilder builder)
        {
            builder.Entity<UserSubscription>()
                .HasKey(us => new { us.AppUserId, us.SubscriptionId });

            builder.Entity<UserSubscription>()
                .HasOne(us => us.AppUser)
                .WithMany(u => u.UserSubscriptions)
                .HasForeignKey(us => us.AppUserId);

            builder.Entity<UserSubscription>()
                .HasOne(us => us.Subscription)
                .WithMany(s => s.UserSubscriptions)
                .HasForeignKey(us => us.SubscriptionId);
        }



    }
}
