using _10_Code_first.Entites;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace _10_Code_first.Data.Config;

public class InstructorConfiguration : IEntityTypeConfiguration<Instructor>
{
    public void Configure(EntityTypeBuilder<Instructor> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();

        builder.Property(x=> x.FirstName)
            .HasColumnType("VARCHAR")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.LastName)
            .HasColumnType("VARCHAR")
            .HasMaxLength(50)
            .IsRequired();

        builder.ToTable("Instructors");

        builder.HasData(LoadInstructors());
    }

    private static List<Instructor> LoadInstructors() => new()
            {
                new Instructor { Id = 1, FirstName = "Ahmed", LastName = "Abdullah"},
                new Instructor { Id = 2, FirstName = "Yasmeen", LastName = "Yasmeen"},
                new Instructor { Id = 3, FirstName = "Khalid", LastName = "Hassan"},
                new Instructor { Id = 4, FirstName = "Nadia", LastName = "Ali"},
                new Instructor { Id = 5, FirstName = "Ahmed", LastName = "Abdallah"},
            };
}