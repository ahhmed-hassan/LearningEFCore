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

        builder.HasMany(Instructor => Instructor.Sections)
            .WithOne()
            .HasForeignKey(sec=> sec.InstructorId)
            .IsRequired(false);

        // OwnsOne: Address is stored as columns IN the Instructors table (not a separate table)
        builder.OwnsOne(i => i.Address, addressBuilder =>
        {
            addressBuilder.Property(a => a.Street)
                .HasColumnName("Address_Street")
                .HasColumnType("VARCHAR")
                .HasMaxLength(200);

            addressBuilder.Property(a => a.City)
                .HasColumnName("Address_City")
                .HasColumnType("VARCHAR")
                .HasMaxLength(100);

            addressBuilder.Property(a => a.ZipCode)
                .HasColumnName("Address_ZipCode")
                .HasColumnType("VARCHAR")
                .HasMaxLength(10);
        });

        builder.ToTable("Instructors");

        builder.HasData(LoadInstructors());

        // Seed data for owned types requires anonymous objects with the owner's PK
        builder.OwnsOne(i => i.Address).HasData(
            new { InstructorId = 1, Street = "123 Main St", City = "Cairo", ZipCode = "11511" },
            new { InstructorId = 2, Street = "456 Elm Ave", City = "Alexandria", ZipCode = "21500" },
            new { InstructorId = 3, Street = "789 Oak Blvd", City = "Cairo", ZipCode = "11765" }
            // Instructors 4 and 5 have no address (NULL)
        );
    }

    private static List<Instructor> LoadInstructors() => new()
            {
                new Instructor { Id = 1, FirstName = "Ahmed", LastName = "Abdullah", OfficeId = 1},
                new Instructor { Id = 2, FirstName = "Yasmeen", LastName = "Yasmeen", OfficeId = 2},
                new Instructor { Id = 3, FirstName = "Khalid", LastName = "Hassan", OfficeId = 3},
                new Instructor { Id = 4, FirstName = "Nadia", LastName = "Ali", OfficeId = 4},
                new Instructor { Id = 5, FirstName = "Ahmed", LastName = "Abdallah", OfficeId = 5},
            };
}