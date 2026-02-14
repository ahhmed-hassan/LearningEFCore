
using _10_Code_first.Entites;
using _10_Code_first.Entites.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace _10_Code_first.Data.Config;

internal class SectionConfiguration : IEntityTypeConfiguration<Section>
{
    public void Configure(EntityTypeBuilder<Section> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).ValueGeneratedNever();
        builder.Property(x => x.SectionName)
            .HasColumnType("VARCHAR")
            .HasMaxLength(255)
            .IsRequired();

        // OwnsOne: Schedule is stored as columns IN the Sections table
        builder.OwnsOne(s => s.Schedule, scheduleBuilder =>
        {
            scheduleBuilder.Property(d => d.StartDate)
                .HasColumnName("Schedule_StartDate")
                .IsRequired();

            scheduleBuilder.Property(d => d.EndDate)
                .HasColumnName("Schedule_EndDate")
                .IsRequired();

            // Explicitly ignore computed properties — EF must not try to map them
            scheduleBuilder.Ignore(d => d.TotalDays);
            scheduleBuilder.Ignore(d => d.IsActive);
        });

        builder.ToTable("Sections");

        builder.HasData(LoadSections());

        // Seed data for owned type — anonymous objects with the owner's PK
        builder.OwnsOne(s => s.Schedule).HasData(
            new { SectionId = 1,  StartDate = new DateTime(2026, 1, 15), EndDate = new DateTime(2026, 4, 15) },
            new { SectionId = 2,  StartDate = new DateTime(2026, 2, 1),  EndDate = new DateTime(2026, 5, 1) },
            new { SectionId = 3,  StartDate = new DateTime(2026, 1, 10), EndDate = new DateTime(2026, 2, 10) },
            new { SectionId = 4,  StartDate = new DateTime(2026, 3, 1),  EndDate = new DateTime(2026, 6, 1) },
            new { SectionId = 5,  StartDate = new DateTime(2026, 1, 20), EndDate = new DateTime(2026, 3, 20) },
            new { SectionId = 6,  StartDate = new DateTime(2026, 4, 1),  EndDate = new DateTime(2026, 7, 1) },
            new { SectionId = 7,  StartDate = new DateTime(2025, 9, 1),  EndDate = new DateTime(2025, 12, 1) },
            new { SectionId = 8,  StartDate = new DateTime(2026, 2, 15), EndDate = new DateTime(2026, 5, 15) },
            new { SectionId = 9,  StartDate = new DateTime(2026, 1, 5),  EndDate = new DateTime(2026, 7, 5) },
            new { SectionId = 10, StartDate = new DateTime(2026, 3, 10), EndDate = new DateTime(2026, 8, 10) },
            new { SectionId = 11, StartDate = new DateTime(2025, 6, 1),  EndDate = new DateTime(2025, 9, 1) }
        );
    }

    private static List<Section> LoadSections() => new()
            {
                new Section { Id = 1, SectionName = "S_MA1", CourseId = 1, InstructorId = 1},
                new Section { Id = 2, SectionName = "S_MA2", CourseId = 1, InstructorId = 2},
                new Section { Id = 3, SectionName = "S_PH1", CourseId = 2, InstructorId = 1},
                new Section { Id = 4, SectionName = "S_PH2", CourseId = 2, InstructorId = 3},
                new Section { Id = 5, SectionName = "S_CH1", CourseId = 3, InstructorId =2},
                new Section { Id = 6, SectionName = "S_CH2", CourseId = 3, InstructorId = 3},
                new Section { Id = 7, SectionName = "S_BI1", CourseId = 4, InstructorId = 4},
                new Section { Id = 8, SectionName = "S_BI2", CourseId = 4, InstructorId = 5},
                new Section { Id = 9, SectionName = "S_CS1", CourseId = 5, InstructorId = 4},
                new Section { Id = 10, SectionName = "S_CS2", CourseId = 5, InstructorId = 5},
                new Section { Id = 11, SectionName = "S_CS3", CourseId = 5, InstructorId = 4}
            };

}
