using EFTest.Domain.Blog;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EFTest.Data.BlogDB.Configurations;

internal class CommentConfiguration : IEntityTypeConfiguration<Comment>
{
    public void Configure(EntityTypeBuilder<Comment> builder)
    {
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Text)
            .IsRequired()
            .HasMaxLength(500);

        // Relationship to Post - already configured in PostConfiguration

        // Relationship to User (Author)
        builder.HasOne(c => c.Author)
            .WithMany() // User doesn't have a Comments navigation property
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.Restrict); // Don't cascade delete comments when user is deleted
    }
}

