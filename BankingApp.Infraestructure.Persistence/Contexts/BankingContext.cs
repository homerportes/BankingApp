using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace BankingApp.Infraestructure.Persistence.Contexts
{
    public class BankingContext : DbContext
    {
        public BankingContext(DbContextOptions<BankingContext> options) :base(options) 
        
        { 

        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        }
     
        
            
        
    }
}
