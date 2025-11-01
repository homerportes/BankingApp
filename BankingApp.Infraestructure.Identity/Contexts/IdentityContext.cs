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
