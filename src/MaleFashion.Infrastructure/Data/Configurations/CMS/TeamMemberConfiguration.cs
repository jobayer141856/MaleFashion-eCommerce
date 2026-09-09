using MaleFashion.Domain.Entities.CMS;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace MaleFashion.Infrastructure.Data.Configurations.CMS
{
    public class TeamMemberConfiguration : IEntityTypeConfiguration<TeamMember>
    {
        public void Configure(EntityTypeBuilder<TeamMember> builder)
        {
            builder.ToTable("TeamMembers");
            builder.HasKey(t => t.Id);
            builder.Property(t => t.Name).HasMaxLength(150).IsRequired();
            builder.Property(t => t.Role).HasMaxLength(150).IsRequired();
            builder.Property(t => t.PhotoUrl).HasMaxLength(500).IsRequired();
        }
    }

}
