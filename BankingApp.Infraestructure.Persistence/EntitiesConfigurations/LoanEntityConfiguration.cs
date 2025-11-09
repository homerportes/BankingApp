using BankingApp.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankingApp.Infraestructure.Persistence.EntitiesConfigurations
{
    public class LoanEntityConfiguration : IEntityTypeConfiguration<Loan>
    {
        public void Configure(EntityTypeBuilder<Loan> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.TotalLoanAmount).IsRequired().HasPrecision(18, 2);
            builder.Property(x => x.ClientId).IsRequired();
            builder.Property(x => x.PublicId).IsRequired();
            builder.Property(x => x.OutstandingBalance).IsRequired().HasPrecision(18, 2);
            builder.Property(x => x.InterestRate).IsRequired();
            builder.Property(x => x.LoanTermInMonths).IsRequired();
            builder.Property(x => x.Status).IsRequired();


            builder.HasMany(x => x.Installments)
                .WithOne(x => x.Loan)
                .HasForeignKey(x => x.Id);
        }
    }
}
