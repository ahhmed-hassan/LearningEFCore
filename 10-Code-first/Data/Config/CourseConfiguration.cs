using _10_Code_first.Entites;
using _10_Code_first.Entites.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Collections.Generic;

namespace _10_Code_first.Data.Config;

public class CourseConfiguration : IEntityTypeConfiguration<Course>
{
    public void Configure(EntityTypeBuilder<Course> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();

        builder.Property(x => x.CourseName)
            .HasColumnType("VARCHAR")
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(x => x.Price)
            .HasPrecision(15, 2);

        // Value converter: CourseTag <-> string
        // EF Core stores the Tag.Value string in the DB and reconstructs CourseTag on read
        builder.Property(x => x.Tag)
            .HasConversion(
                tag => tag.Value,           // C# -> DB: store the string value
                value => new CourseTag(value) // DB -> C#: reconstruct from string
            )
            .HasColumnType("VARCHAR")
            .HasMaxLength(50)
            .IsRequired();

        builder.HasMany(course => course.Sections)
            .WithOne()
            .HasForeignKey(section => section.CourseId)
            .IsRequired();

        builder.ToTable("Courses");

        builder.HasData(LoadCourses());
    }

    private List<Course> LoadCourses() =>
         new()
            {
                new Course { Id = 1, CourseName = "Mathmatics", Price = 1000m, Tag = CourseTag.Beginner },
                new Course { Id = 2, CourseName = "Physics", Price = 2000m, Tag = CourseTag.Advanced },
                new Course { Id = 3, CourseName = "Chemistry", Price = 1500m, Tag = CourseTag.Intermediate },
                new Course { Id = 4, CourseName = "Biology", Price = 1200m, Tag = CourseTag.Beginner },
                new Course { Id = 5, CourseName = "CS-50", Price = 3000m, Tag = CourseTag.Advanced },
            };
}
