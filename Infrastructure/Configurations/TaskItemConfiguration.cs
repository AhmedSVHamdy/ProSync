using Core.Domain.Entities;
using Core.Domain.RepositoryContracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.Configurations
{
    public class TaskItemConfiguration : TenantEntityConfiguration<TaskItem>
    {
        public override void Configure(EntityTypeBuilder<TaskItem> builder)
        {
            base.Configure(builder);
            builder.ToTable("TaskItems");
            builder.HasKey(t => t.Id);
            builder.Property(t => t.Title)
                .IsRequired()
                .HasMaxLength(200);
            builder.Property(t => t.Description)
                .IsRequired()
                .HasMaxLength(2000);
            builder.Property(t => t.PullRequestUrl)
                .IsRequired()
                .HasMaxLength(500);
            builder.Property(t => t.Status)
                .HasConversion<string>()
                .IsRequired()
                .HasMaxLength(50);
            builder.HasOne(t => t.Project)           
                .WithMany()
                .HasForeignKey(t => t.ProjectId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(t => t.Sprint)            
                .WithMany()
                .HasForeignKey(t => t.SprintId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(t => t.Assignee)          
                .WithMany()
                .HasForeignKey(t => t.AssigneeId)
                .OnDelete(DeleteBehavior.Restrict);

        }
    }

}
