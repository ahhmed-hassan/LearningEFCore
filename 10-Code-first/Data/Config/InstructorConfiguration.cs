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

        builder.Property(x=> x.Name)
            .HasColumnType("VARCHAR")
            .HasMaxLength(255)
            .IsRequired();


        builder.ToTable("Instructors");

        builder.HasData(LoadInstructors());
    }

    private static List<Instructor> LoadInstructors() => new()
            {
                new Instructor { Id = 1, Name = "Ahmed Abdullah"},
                new Instructor { Id = 2, Name = "Yasmeen Yasmeen"},
                new Instructor { Id = 3, Name = "Khalid Hassan"},
                new Instructor { Id = 4, Name = "Nadia Ali"},
                new Instructor { Id = 5, Name = "Ahmed Abdullah"},
            };
}