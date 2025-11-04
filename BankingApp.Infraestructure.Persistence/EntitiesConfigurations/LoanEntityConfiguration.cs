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
            builder.Property(x => x.TotalLoanAmount).IsRequired();
            builder.Property(x => x.ClientId).IsRequired();
            builder.Property(x => x.TotalInstallmentsCount).IsRequired();
            builder.Property(x => x.PaidInstallmentsCount).IsRequired();
            builder.Property(x => x.OutstandingBalance).IsRequired();
            builder.Property(x => x.InterestRate).IsRequired();
            builder.Property(x => x.LoanTermInMonths).IsRequired();
            builder.Property(x => x.Status).IsRequired();

        }
    }
}
