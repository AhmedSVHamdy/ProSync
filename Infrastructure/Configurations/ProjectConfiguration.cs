using Core.Domain.Entities;
using Core.Domain.RepositoryContracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.Configurations
{
    public class ProjectConfiguration : TenantEntityConfiguration<Project>
    {
        public override void Configure(EntityTypeBuilder<Project> builder)
        {
            base.Configure(builder);
            builder.ToTable("Projects");
            builder.HasKey(p => p.Id);

            builder.Property(p => p.Name)
                .IsRequired()
                .HasMaxLength(150);
            builder.Property(p => p.Status)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength (50);
        }
    }
}
