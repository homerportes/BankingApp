using BankingApp.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;


namespace BankingApp.Infraestructure.Persistence.EntitiesConfigurations
{
    public class InstallmentEntityConfiguration : IEntityTypeConfiguration<Installment>
    {
        public void Configure(EntityTypeBuilder<Installment> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).ValueGeneratedOnAdd();
            
            builder.Property(x => x.PayDate).IsRequired();
            builder.Property(x => x.Value).IsRequired().HasPrecision(18, 2);
            builder.Property(x => x.Number).IsRequired();

            // Configurar la relación con Loan
            builder.HasOne(i => i.Loan)
                   .WithMany(l => l.Installments)
                   .HasForeignKey(i => i.LoanId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
