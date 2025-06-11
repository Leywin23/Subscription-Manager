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

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            SeedRoles(builder);
            ConfigureSubscription(builder);
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
        private void ConfigureSubscription(ModelBuilder builder) {
            builder.Entity<Subscription>()
                .HasOne(s=>s.AppUser)
                .WithMany(u=>u.Subscriptions)
                .HasForeignKey(s=>s.AppUserId)
                .OnDelete(DeleteBehavior.Restrict);
        }


    }
}
