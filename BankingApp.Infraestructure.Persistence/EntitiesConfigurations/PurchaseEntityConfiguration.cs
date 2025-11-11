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
    public class PurchaseEntityConfiguration : IEntityTypeConfiguration<Purchase>
    {
        public void Configure(EntityTypeBuilder<Purchase> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.AmountSpent).IsRequired().HasPrecision(18, 2);
            builder.Property(x => x.MerchantName).IsRequired();
            builder.Property(x => x.DateTime).IsRequired();
            builder.Property(x => x.CardNumber).IsRequired();
            builder.Property(x => x.Status).IsRequired().HasConversion<string>();
        }
    }
}
