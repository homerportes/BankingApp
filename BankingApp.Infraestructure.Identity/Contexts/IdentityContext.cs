using BankingApp.Infraestructure.Identity.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankingApp.Infraestructure.Identity.Contexts
{
    
    
        public class IdentityContext : IdentityDbContext<AppUser>
        {
       

        public IdentityContext(DbContextOptions<IdentityContext> options) : base(options)
            {

            }

        public new DbSet<AppUser> Users => Set<AppUser>();
        public new DbSet<IdentityRole> Roles => Set<IdentityRole>();
        public new DbSet<IdentityUserRole<string>> UserRoles => Set<IdentityUserRole<string>>();
        public new DbSet<IdentityUserLogin<string>> UserLogins => Set<IdentityUserLogin<string>>();


        protected override void OnModelCreating(ModelBuilder builder)
            {
                base.OnModelCreating(builder);

                builder.HasDefaultSchema("Identity");
                builder.Entity<AppUser>().ToTable("Users");
                builder.Entity<IdentityRole>().ToTable("Roles");
                builder.Entity<IdentityUserRole<string>>().ToTable("UserRoles");
                builder.Entity<IdentityUserLogin<string>>().ToTable("UserLogins");

            }

        }
    
}
