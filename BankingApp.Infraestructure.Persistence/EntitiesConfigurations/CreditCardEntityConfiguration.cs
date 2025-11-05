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
    public class CreditCardEntityConfiguration : IEntityTypeConfiguration<CreditCard>
    {
        public void Configure(EntityTypeBuilder<CreditCard> builder)
        {
           builder.HasKey(x=>x.Id);
            builder.Property(x=>x.ClientId).IsRequired();
            builder.Property(x => x.AdminId).IsRequired();
            builder.Property(x => x.Status).IsRequired();
            builder.Property(x => x.CreditLimitAmount).IsRequired();
            builder.Property(x => x.CVC).IsRequired();
            builder.Property(x => x.ExpirationDate).IsRequired();
            builder.Property(x => x.TotalAmountOwed).IsRequired();


            builder.HasMany(x => x.Purchases)
                    .WithOne(x => x.CreditCard)
                    .HasForeignKey(x => x.CardId);

        }
    }
}
