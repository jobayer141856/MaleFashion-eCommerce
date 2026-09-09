using MaleFashion.Domain.Entities.CMS;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace MaleFashion.Infrastructure.Data.Configurations.CMS
{
    public class PartnerClientConfiguration : IEntityTypeConfiguration<PartnerClient>
    {
        public void Configure(EntityTypeBuilder<PartnerClient> builder)
        {
            builder.ToTable("PartnerClients");
            builder.HasKey(p => p.Id);
            builder.Property(p => p.ClientName).HasMaxLength(150).IsRequired();
            builder.Property(p => p.LogoUrl).HasMaxLength(500).IsRequired();
        }
    }
}
