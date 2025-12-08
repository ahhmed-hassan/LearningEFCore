using _10_Code_first.Entites;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace _10_Code_first.Data.Config;

internal class OfficeConfiguration : IEntityTypeConfiguration<Office>
{
    public void Configure(EntityTypeBuilder<Office> builder)
    {

        builder.Property(off => off.officeName)
            .HasColumnType("VARCHAR")
            .HasMaxLength(255)
            .IsRequired();

        builder.ToTable("Offices");
        builder.HasData(LoadOffices().Value);

        builder.HasKey(off => off.Id);
        builder.Property(off => off.Id).ValueGeneratedNever();

        builder.HasOne(off => off.instructor)
            .WithOne()
            .HasForeignKey<Instructor>(Instructor => Instructor.OfficeId)
            .IsRequired(false);
    }

    private static Lazy<List<Office>> LoadOffices() => new(() =>
        new List<Office>
        {
            new Office { Id = 1, officeName = "Main Office" },
            new Office { Id = 2, officeName = "Branch Office" },
            new Office { Id = 3, officeName = "Remote Office" },
            new Office { Id = 4, officeName = "Headquarters" },
            new Office { Id = 5, officeName = "Satellite Office" },
        });


}

