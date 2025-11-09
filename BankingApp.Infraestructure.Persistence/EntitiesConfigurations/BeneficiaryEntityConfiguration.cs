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
    internal class BeneficiaryEntityConfiguration : IEntityTypeConfiguration<Beneficiary>
    {

        public void Configure(EntityTypeBuilder<Beneficiary> builder)
        {


            #region basic configurations
            builder.ToTable("Beneficiarys");
            builder.HasKey(x => x.Id);
            #endregion



            #region property configuration
            builder.Property( b => b.BeneficiaryId).IsRequired().HasMaxLength(450);
            builder.Property( b => b.ClientId).IsRequired().HasMaxLength(450);
            #endregion


            #region relationship configurations
            // no tendra coomo tal
            #endregion

        }
    }
}
